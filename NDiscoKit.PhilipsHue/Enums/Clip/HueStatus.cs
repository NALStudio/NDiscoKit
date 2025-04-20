using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Enums.Clip;


[JsonConverter(typeof(JsonStringEnumConverter<HueStatus>))]
public enum HueStatus
{
    [JsonStringEnumMemberName("active")]
    Active,

    [JsonStringEnumMemberName("inactive")]
    Inactive
}
