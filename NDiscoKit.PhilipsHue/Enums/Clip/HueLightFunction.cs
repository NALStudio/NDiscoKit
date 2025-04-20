using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Enums.Clip;

[JsonConverter(typeof(JsonStringEnumConverter<HueLightFunction>))]
public enum HueLightFunction
{
    [JsonStringEnumMemberName("functional")]
    Functional,

    [JsonStringEnumMemberName("decorative")]
    Decorative,

    [JsonStringEnumMemberName("mixed")]
    Mixed,

    [JsonStringEnumMemberName("unknown")]
    Unknown
}
