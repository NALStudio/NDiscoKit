using NDiscoKit.PhilipsHue.Enums.Clip;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueLightProductDataGet
{
    public string? Name { get; init; }
    public string? Archetype { get; init; }
    public HueLightFunction Function { get; init; }
}
