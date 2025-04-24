using NDiscoKit.PhilipsHue.Enums;
using NDiscoKit.PhilipsHue.Models.Clip.Generic;
using System.Collections.Immutable;

namespace NDiscoKit.PhilipsHue.Models.Clip.Put;
public sealed class HueLightGradientPut
{
    public required ImmutableArray<GradientPointPut> Points { get; set; }

    /// <summary>
    /// If not provided during PUT/POST it will be defaulted to interpolated_palette.
    /// </summary>
    public HueGradientMode? Mode { get; set; }
}
