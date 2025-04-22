using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public readonly struct HueLightMirekSchemaGet
{
    [JsonPropertyName("mirek_minimum")]
    public required int Minimum { get; init; }

    [JsonPropertyName("mirek_maximum")]
    public required int Maximum { get; init; }
}
