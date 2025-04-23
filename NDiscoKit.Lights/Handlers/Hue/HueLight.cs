using NDiscoKit.Lights.Models.Color;
using NDiscoKit.PhilipsHue.Api;

namespace NDiscoKit.Lights.Handlers.Hue;
public class HueLight : Light
{
    private readonly LocalHueApi hue;

    public byte ChannelId { get; }

    public Guid? LightId { get; }
    public string? LightArchetype { get; }

    internal HueLight(LocalHueApi hue, byte channelId, Guid? lightId, string? lightArchetype)
    {
        this.hue = hue;
        ChannelId = channelId;
        LightId = lightId;
        LightArchetype = lightArchetype;
    }

    public override bool CanSignal => LightId.HasValue;

    public override ValueTask<bool> Signal(TimeSpan duration, NDKColor color)
    {
        // if (!LightId.HasValue)
        //     return false;

        throw new NotImplementedException();
    }
}
