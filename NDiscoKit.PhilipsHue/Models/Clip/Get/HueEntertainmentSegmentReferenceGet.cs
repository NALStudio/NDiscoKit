using NDiscoKit.PhilipsHue.Models.Clip.Generic;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueEntertainmentSegmentReferenceGet
{
    public required HueResourceIdentifier Service { get; init; }
    public required int Index { get; init; }
}
