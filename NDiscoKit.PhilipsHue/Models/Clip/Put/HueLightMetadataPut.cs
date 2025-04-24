using NDiscoKit.PhilipsHue.Enums.Clip;

namespace NDiscoKit.PhilipsHue.Models.Clip.Put;
public sealed class HueLightMetadataPut
{
    public string? Name { get; set; }

    [Obsolete("use metadata on device level")]
    public string? Archetype { get; set; }

    public HueLightFunction? Function { get; set; }
}
