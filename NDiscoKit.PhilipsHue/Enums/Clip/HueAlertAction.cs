using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Enums.Clip;

[JsonConverter(typeof(JsonStringEnumConverter<HueAlertAction>))]
public enum HueAlertAction
{
    [JsonStringEnumMemberName("breathe")]
    Breathe
}
