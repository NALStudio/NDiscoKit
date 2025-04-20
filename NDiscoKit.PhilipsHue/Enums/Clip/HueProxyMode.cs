using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Enums.Clip;

[JsonConverter(typeof(JsonStringEnumConverter<HueProxyMode>))]
public enum HueProxyMode
{
    [JsonStringEnumMemberName("auto")]
    Auto,

    [JsonStringEnumMemberName("manual")]
    Manual
}
