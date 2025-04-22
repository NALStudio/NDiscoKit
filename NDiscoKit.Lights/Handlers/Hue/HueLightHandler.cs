using NDiscoKit.Lights.Helpers;
using NDiscoKit.Lights.Models;
using NDiscoKit.PhilipsHue.Api;
using NDiscoKit.PhilipsHue.Models.Clip.Get;
using NDiscoKit.PhilipsHue.Models.Entertainment.Channels;
using System.Collections.Immutable;
using System.Diagnostics;

namespace NDiscoKit.Lights.Handlers.Hue;
public class HueLightHandler : LightHandler
{
    // Implemented for future-proofing
    // Currently I can't find any info on Philips Hue Entertainment latency,
    // but this can be adjusted later if I observe any latency IRL.
    private static readonly TimeSpan _kExpectedLatency = TimeSpan.Zero;

    private bool disposed = false;

    private HueEntertainment? entertainment;

    private readonly bool disposeHue;
    public LocalHueApi Hue { get; }
    public Guid EntertainmentArea { get; }

    public override ImmutableArray<Light> Lights { get; }
    private readonly HueXYEntertainmentChannel[] channels;

    public HueLightHandler(LocalHueApi hue, bool disposeHue, Guid entertainmentArea, IEnumerable<Light> lights)
    {
        this.disposeHue = disposeHue;
        Hue = hue;
        EntertainmentArea = entertainmentArea;

        Lights = lights.ToImmutableArray();
        channels = new HueXYEntertainmentChannel[Lights.Length];
    }

    public static async Task<HueLightHandler?> TryCreateAsync(LocalHueApi hue, Guid entertainmentAreaId, bool disposeHue = false)
    {
        try
        {
            HueEntertainmentConfigurationGet ent = await hue.GetEntertainmentConfigurationAsync(entertainmentAreaId);

            List<Light> lights = new();
            foreach (HueEntertainmentChannelGet c in ent.Channels)
                lights.Add(await GetLight(hue, ent, c));

            return new HueLightHandler(hue, disposeHue, entertainmentAreaId, lights);
        }
        catch
        {
            if (disposeHue)
                hue.Dispose();
            throw;
        }
    }

    private static async Task<HueLight> GetLight(LocalHueApi hue, HueEntertainmentConfigurationGet entertainmentConfiguration, HueEntertainmentChannelGet channel)
    {
        static ColorGamut GamutFromHue(HueLightColorGamutGet gamut)
        {
            return new ColorGamut(
                red: new ColorGamutPoint(gamut.Red.X, gamut.Red.Y),
                green: new ColorGamutPoint(gamut.Green.X, gamut.Green.Y),
                blue: new ColorGamutPoint(gamut.Blue.X, gamut.Blue.Y)
            );
        }

        HueLightGet? light = null;
        if (channel.Members.Length == 1)
        {
            var service = channel.Members[0].Service;
            if (service.ResourceType.IsEntertainmentService)
                light = await hue.GetLightAsync(service.ResourceId);
        }

        return new HueLight(hue, channel.ChannelId, light?.Id)
        {
            DisplayName = light?.Metadata.Name,
            Position = new LightPosition(channel.Position.X, channel.Position.Y, channel.Position.Z),
            ColorGamut = light?.Color?.Gamut is HueLightColorGamutGet g ? GamutFromHue(g) : null,
            ExpectedLatency = _kExpectedLatency
        };
    }

    public override async ValueTask<bool> Start()
    {
        entertainment = await HueEntertainment.ConnectAsync(Hue, EntertainmentArea);
        return entertainment is not null;
    }

    public override ValueTask Update()
    {
        if (entertainment is null)
            throw new InvalidOperationException("Hue Light Handler not started.");

        ReadOnlySpan<Light> lights = Lights.AsSpan();
        Span<HueXYEntertainmentChannel> channels = this.channels.AsSpan();
        Debug.Assert(lights.Length == channels.Length);

        for (int i = 0; i < lights.Length; i++)
        {
            HueLight light = (HueLight)lights[i];
            channels[i] = new HueXYEntertainmentChannel(light.ChannelId)
            {
                X = BitResolution.AsUInt16(light.Color.X),
                Y = BitResolution.AsUInt16(light.Color.Y),
                Brightness = BitResolution.AsUInt16(light.Color.Brightness),
            };
        }

        entertainment.Send(channels);
        return ValueTask.CompletedTask;
    }

    public override ValueTask Stop()
    {
        entertainment?.Dispose();
        return ValueTask.CompletedTask;
    }

    public override ValueTask DisposeAsync()
    {
        if (disposed)
            return ValueTask.CompletedTask;

        if (disposeHue)
            Hue.Dispose();

        disposed = true;
        return ValueTask.CompletedTask;
    }
}
