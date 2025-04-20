using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Enums.Clip;

[JsonConverter(typeof(JsonStringEnumConverter<HueEntertainmentConfigurationType>))]
public enum HueEntertainmentConfigurationType
{
    /// <summary>
    /// Channels are organized around content from a screen.
    /// </summary>
    [JsonStringEnumMemberName("screen")]
    Screen,

    /// <summary>
    /// Channels are organized around content from one or several monitors.
    /// </summary>
    [JsonStringEnumMemberName("monitor")]
    Monitor,

    /// <summary>
    /// Channels are organized for music synchronization.
    /// </summary>
    [JsonStringEnumMemberName("music")]
    Music,

    /// <summary>
    /// Channels are organized to provide 3d spacial effects.
    /// </summary>
    [JsonStringEnumMemberName("3dspace")]
    Space,

    /// <summary>
    /// General use case.
    /// </summary>
    [JsonStringEnumMemberName("other")]
    Other,
}
