using NDiscoKit.PhilipsHue.Models.Clip.Generic;
using System.Collections.Immutable;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueEntertainmentChannelGet
{
    public required byte ChannelId { get; init; }
    public required HuePosition Position { get; init; }
    public required ImmutableArray<HueSegmentReferenceGet> Members { get; init; }
}
