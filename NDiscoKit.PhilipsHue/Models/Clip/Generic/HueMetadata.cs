using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Models.Clip.Generic;
public class HueMetadata
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
