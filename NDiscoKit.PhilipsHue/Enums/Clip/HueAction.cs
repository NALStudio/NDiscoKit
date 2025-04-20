using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Enums.Clip;

[JsonConverter(typeof(JsonStringEnumConverter<HueAction>))]
public enum HueAction
{
    [JsonStringEnumMemberName("start")]
    Start,

    [JsonStringEnumMemberName("stop")]
    Stop
}
