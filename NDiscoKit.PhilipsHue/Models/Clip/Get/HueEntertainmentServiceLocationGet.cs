using NDiscoKit.PhilipsHue.Models.Clip.Generic;
using System.Collections.Immutable;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueEntertainmentServiceLocationGet
{
    public required HueResourceIdentifier Service { get; init; }
    public ImmutableArray<HuePosition> Positions { get; init; }
    public required double EqualizationFactor { get; init; }
}
