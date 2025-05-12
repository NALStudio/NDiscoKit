using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Enums.Clip;

public enum HueGradientMode
{
    [JsonStringEnumMemberName("interpolated_palette")]
    InterpolatedPalette,

    [JsonStringEnumMemberName("interpolated_palette_mirrored")]
    InterpolatedPaletteMirrored,

    [JsonStringEnumMemberName("random_pixelated")]
    RandomPixelated
}
