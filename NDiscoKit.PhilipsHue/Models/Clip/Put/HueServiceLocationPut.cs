using NDiscoKit.PhilipsHue.Models.Clip.Generic;
using System.Collections.Immutable;

namespace NDiscoKit.PhilipsHue.Models.Clip.Put;
public sealed class HueServiceLocationPut
{
    public required HueResourceIdentifier Service { get; set; }
    public required ImmutableArray<HuePosition> Positions { get; set; }
    public double EqualizationFactor { get; set; }
}
