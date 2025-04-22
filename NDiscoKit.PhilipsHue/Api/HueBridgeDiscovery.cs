using NDiscoKit.PhilipsHue.Models;
using Zeroconf;

namespace NDiscoKit.PhilipsHue.Api;

public static class HueBridgeDiscovery
{
    /// <summary>
    /// Search bridges using Multicast.
    /// </summary>
    /// <param name="OnBridgeDiscovered">
    /// Return any discovered bridges early through a callback. This callback may come from a different thread.
    /// </param>
    public static async Task<IEnumerable<DiscoveredBridge>> Multicast(TimeSpan scanTime, Action<DiscoveredBridge>? OnBridgeDiscovered = null, CancellationToken cancellationToken = default)
    {
        static DiscoveredBridge ConvertToBridge(IZeroconfHost host)
        {
            return new()
            {
                Name = host.DisplayName,
                BridgeId = null,
                IpAddress = host.IPAddress
            };
        }

        Action<IZeroconfHost>? resolveCallback = null;
        if (OnBridgeDiscovered is not null)
            resolveCallback = host => OnBridgeDiscovered(ConvertToBridge(host));

        IReadOnlyList<IZeroconfHost> result = await ZeroconfResolver.ResolveAsync("_hue._tcp.local.", scanTime: scanTime, callback: resolveCallback, cancellationToken: cancellationToken);
        return result.Select(static r => ConvertToBridge(r));
    }

    /// <inheritdoc cref="Multicast(TimeSpan, Action{DiscoveredBridge}?)"/>
    public static Task<IEnumerable<DiscoveredBridge>> Multicast(int scanTimeMs = 10_000, Action<DiscoveredBridge>? OnBridgeDiscovered = null, CancellationToken cancellationToken = default)
        => Multicast(TimeSpan.FromMilliseconds(scanTimeMs), OnBridgeDiscovered: OnBridgeDiscovered, cancellationToken: cancellationToken);

    /// <summary>
    /// <b>NOT IMPLEMENTED</b>
    /// </summary>
    public static Task<DiscoveredBridge> Endpoint()
    {
        throw new NotImplementedException();
    }
}
