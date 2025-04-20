using NDiscoKit.PhilipsHue.Enums.Clip;
using NDiscoKit.PhilipsHue.Models.Clip.Generic;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueEntertainmentStreamProxyGet
{
    public required HueProxyMode Mode { get; init; }
    public required HueResourceIdentifier Node { get; init; }
}
