using NDiscoKit.PhilipsHue.Enums.Clip;
using NDiscoKit.PhilipsHue.Models.Clip.Generic;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueLightColorGet
{
    public required HueXY XY { get; init; }
    public HueLightColorGamutGet? Gamut { get; init; }
    public HueColorGamut GamutType { get; init; }
}
