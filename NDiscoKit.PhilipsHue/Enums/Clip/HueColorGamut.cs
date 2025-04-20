using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Enums.Clip;

[JsonConverter(typeof(JsonStringEnumConverter<HueColorGamut>))]
public enum HueColorGamut
{
    /// <summary>
    /// Gamut of early Philips color-only products.
    /// </summary>
    [JsonStringEnumMemberName("A")]
    A,

    /// <summary>
    /// Limited gamut of first Hue color products.
    /// </summary>
    [JsonStringEnumMemberName("B")]
    B,

    /// <summary>
    /// Richer color gamut of Hue white and color ambiance products.
    /// </summary>
    [JsonStringEnumMemberName("C")]
    C,

    /// <summary>
    /// Color gamut of non-hue products with non-hue gamuts resp w/o gamut.
    /// </summary>
    [JsonStringEnumMemberName("other")]
    Other
}
