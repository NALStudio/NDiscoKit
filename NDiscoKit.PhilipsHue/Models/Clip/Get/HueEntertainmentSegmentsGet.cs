using System.Collections.Immutable;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueEntertainmentSegmentsGet
{
    public required bool Configurable { get; init; }
    public required int MaxSegments { get; init; }
    public required ImmutableArray<HueEntertainmentSegmentGet> Segments { get; init; }
}
