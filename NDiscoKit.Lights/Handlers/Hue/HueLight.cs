using NDiscoKit.PhilipsHue.Api;
using NDiscoKit.PhilipsHue.Models.Clip.Put;

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

    public override bool CanIdentify => LightId.HasValue;
    public override async ValueTask<bool> IdentifyAsync()
    {
        if (!LightId.HasValue)
            return false;

        await hue.UpdateLightAsync(LightId.Value, new HueLightPut() { Identify = true });
        return true;
    }
}
