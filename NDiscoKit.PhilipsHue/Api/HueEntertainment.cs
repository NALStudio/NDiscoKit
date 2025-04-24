using Microsoft.Extensions.Logging;
using NDiscoKit.PhilipsHue.Enums.Clip;
using NDiscoKit.PhilipsHue.Helpers;
using NDiscoKit.PhilipsHue.Models;
using NDiscoKit.PhilipsHue.Models.Clip.Put;
using NDiscoKit.PhilipsHue.Models.Entertainment.Channels;
using NDiscoKit.PhilipsHue.Models.Entertainment.Connection;
using NDiscoKit.PhilipsHue.Models.Exceptions;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NDiscoKit.PhilipsHue.Api;

// Heavy reference: https://github.com/michielpost/Q42.HueApi/blob/e3d059128e11ee19a7cac9fd2265bee989ac4271/src/HueApi.Entertainment/StreamingHueClient.cs
public sealed class HueEntertainment : IDisposable
{
    private const int maxChannelsCount = 20;
    private const int payloadHeaderLength = 52; // includes entertainment configuration id
    private const int maxPayloadLength = payloadHeaderLength + (maxChannelsCount * IHueEntertainmentChannel.BytesPerChannel);

    private readonly Lock sendLock;
    private readonly byte[] payload;
    private byte sequenceId = byte.MaxValue;

    private readonly Socket socket;

    [MemberNotNullWhen(true, nameof(dtls), nameof(udp))]
    private bool Connected { get; set; }

    private DtlsTransport? dtls;
    private UdpTransport? udp;

    private HueEntertainment(Guid entertainmentConfigurationId)
    {
        sendLock = new();

        payload = ConstructDefaultPayload(entertainmentConfigurationId);

        socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Any, 0));
    }

    private static byte[] GetGuidPayloadBytes(Guid guid)
    {
        string guidString = guid.ToString().ToLowerInvariant();
        return Encoding.ASCII.GetBytes(guidString);
    }

    public static async Task<HueEntertainment?> ConnectAsync(string bridgeIp, HueCredentials credentials, Guid entertainmentConfigurationId, ILogger<LocalHueApi>? hueLogger = null)
    {
        using LocalHueApi hue = new(bridgeIp, credentials, hueLogger);
        return await ConnectAsync(hue, entertainmentConfigurationId);
    }

    public static async Task<HueEntertainment?> ConnectAsync(LocalHueApi hue, Guid entertainmentConfigurationId)
    {
        HueEntertainment entertainment = new(entertainmentConfigurationId);
        try
        {
            await hue.UpdateEntertainmentConfigurationAsync(entertainmentConfigurationId, new HueEntertainmentConfigurationPut() { Action = HueStateAction.Start });
            await entertainment.ConnectDtls(hue);
        }
        catch
        {
            entertainment.Dispose();
            return null;
        }

        return entertainment;
    }
    private async Task ConnectDtls(LocalHueApi hue)
    {
        BasicTlsPskIdentity pskIdentity = new(hue.Credentials.AppKey, HexConverter.DecodeHex(hue.Credentials.ClientKey));

        DtlsClient dtlsClient = new(null, pskIdentity);

        DtlsClientProtocol clientProtocol = new(new SecureRandom());
        await socket.ConnectAsync(hue.BridgeIp, 2100); // We connect onto the host IP, not the Uri address

        udp = new UdpTransport(socket);
        dtls = clientProtocol.Connect(dtlsClient, udp);

        sequenceId = byte.MaxValue;

        Connected = true;
    }

    public void Send(ReadOnlySpan<HueRGBEntertainmentChannel> channels) => Send(0x00, channels);
    public void Send(ReadOnlySpan<HueXYEntertainmentChannel> channels) => Send(0x01, channels);

    private void Send<T>(byte colorSpace, ReadOnlySpan<T> channels) where T : IHueEntertainmentChannel
    {
        if (channels.Length > maxChannelsCount)
            throw new ArgumentException($"Maximum 20 slots of color data supported, got: {channels.Length}");

        if (!sendLock.TryEnter())
            throw new InvalidOperationException("Send function is already running on another thread.");
        try
        {
            unchecked // unchecked to allow to roll back to zero after 255
            {
                // sequenceId is initialized with byte.MaxValue so the first value sent to the bridge will be 0
                sequenceId++;
            }

            int length = UpdatePayload(payload, sequenceId, colorSpace, in channels);
            Send(payload, length);
        }
        finally
        {
            sendLock.Exit();
        }
    }

    private static byte[] ConstructDefaultPayload(Guid entertainmentConfiguration)
    {
        byte[] bytesArray = new byte[maxPayloadLength];
        Span<byte> bytes = bytesArray.AsSpan();

        // Protocol name
        bytes[0] = (byte)'H';
        bytes[1] = (byte)'u';
        bytes[2] = (byte)'e';
        bytes[3] = (byte)'S';
        bytes[4] = (byte)'t';
        bytes[5] = (byte)'r';
        bytes[6] = (byte)'e';
        bytes[7] = (byte)'a';
        bytes[8] = (byte)'m';

        // Version 2.0
        bytes[9] = 0x02;
        bytes[10] = 0x00;

        // Sequence ID
        // bytes[11]

        // Reserved
        // bytes[12]
        // bytes[13]

        // Color space
        // bytes[14]

        // Reserved
        // bytes[15] = 0x00;

        byte[] entertainmentConfigurationBytes = GetGuidPayloadBytes(entertainmentConfiguration);
        entertainmentConfigurationBytes.CopyTo(bytes[16..]);

        Debug.Assert(16 + entertainmentConfigurationBytes.Length == payloadHeaderLength, "Invalid header length.");

        return bytesArray;
    }

    private static int UpdatePayload<T>(scoped Span<byte> bytes, byte sequenceId, byte colorSpace, scoped ref readonly ReadOnlySpan<T> channels) where T : IHueEntertainmentChannel
    {
        // Sequence ID
        bytes[11] = sequenceId;

        // Color space
        bytes[14] = colorSpace;

        int length = payloadHeaderLength;
        for (int i = 0; i < channels.Length; i++)
        {
            Span<byte> channel = bytes.Slice(length, IHueEntertainmentChannel.BytesPerChannel);
            channels[i].SetBytes(channel);
            length += IHueEntertainmentChannel.BytesPerChannel;
        }

        return length;
    }

    private void Send(byte[] data, int length)
    {
        if (!Connected) // We should be connected as long as dispose isn't called
            throw new HueEntertainmentException("Entertainment API disposed.");

        dtls.Send(data, off: 0, len: length);
    }

    private void Disconnect()
    {
        Debug.Assert(Connected == socket.Connected);
        if (!Connected)
            return;

        Connected = false;

        dtls.Close();
        dtls = null;

        udp.Close();
        udp = null;

        // https://stackoverflow.com/questions/35229143/what-exactly-do-sockets-shutdown-disconnect-close-and-dispose-do/35229144#35229144
        // We shut down because we don't care whether the unsent packages ever make it to the bridge.
        socket.Shutdown(SocketShutdown.Both);
    }

    public void Dispose()
    {
        Disconnect();
        socket.Dispose();
        GC.SuppressFinalize(this);
    }
}
