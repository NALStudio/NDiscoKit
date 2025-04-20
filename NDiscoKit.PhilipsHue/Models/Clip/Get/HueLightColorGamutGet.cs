using NDiscoKit.PhilipsHue.Models.Clip.Generic;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueLightColorGamutGet
{
    public required HueXY Red { get; init; }
    public required HueXY Green { get; init; }
    public required HueXY Blue { get; init; }
}
