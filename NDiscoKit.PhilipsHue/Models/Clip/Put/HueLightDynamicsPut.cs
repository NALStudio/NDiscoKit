using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Models.Clip.Put;
public sealed class HueLightDynamicsPut
{
    [JsonPropertyName("duration")]
    public int? DurationMs { get; set; }

    public double? Speed { get; set; }
}
