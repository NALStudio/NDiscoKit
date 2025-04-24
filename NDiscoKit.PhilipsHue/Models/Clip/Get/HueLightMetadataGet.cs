using NDiscoKit.PhilipsHue.Enums.Clip;
using NDiscoKit.PhilipsHue.Models.Clip.Generic;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueLightMetadataGet : HueMetadata
{
    [Obsolete("use metadata on device level")]
    public required string Archetype { get; init; }

    public int? FixedMired { get; init; }
    public required HueLightFunction Function { get; init; }
}
