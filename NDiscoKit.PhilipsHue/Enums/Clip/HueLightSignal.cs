using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Enums.Clip;

[JsonConverter(typeof(JsonStringEnumConverter<HueLightSignal>))]
public enum HueLightSignal
{
    [JsonStringEnumMemberName("no_signal")]
    NoSignal,

    [JsonStringEnumMemberName("on_off")]
    OnOff,

    [JsonStringEnumMemberName("on_off_color")]
    OnOffColor,

    [JsonStringEnumMemberName("alternating")]
    Alternating
}
