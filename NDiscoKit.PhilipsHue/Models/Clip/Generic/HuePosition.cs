using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Models.Clip.Generic;
public readonly struct HuePosition
{
    [JsonPropertyName("x")]
    public required double X { get; init; }

    [JsonPropertyName("y")]
    public required double Y { get; init; }

    [JsonPropertyName("z")]
    public required double Z { get; init; }
}
