using NDiscoKit.Lights.Models.Color;
using NDiscoKit.PhilipsHue.Api;

namespace NDiscoKit.Lights.Handlers.Hue;
internal class HueLight : Light
{
    private readonly LocalHueApi hue;

    public byte ChannelId { get; }
    public Guid? LightId { get; }

    internal HueLight(LocalHueApi hue, byte channelId, Guid? lightId)
    {
        this.hue = hue;
        ChannelId = channelId;
        LightId = lightId;
    }

    public override ValueTask<bool> Signal(TimeSpan duration, NDKColor color)
    {
        // if (!LightId.HasValue)
        //     return false;

        throw new NotImplementedException();
    }
}
