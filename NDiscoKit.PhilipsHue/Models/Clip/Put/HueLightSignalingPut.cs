using NDiscoKit.PhilipsHue.Enums.Clip;
using NDiscoKit.PhilipsHue.Models.Clip.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Models.Clip.Put;
public sealed class HueLightSignalingPut
{
    public required HueLightSignal Signal { get; set; }

    [JsonPropertyName("duration")]
    public required int DurationMs { get; set; }

    public ImmutableArray<ColorFeatureBasicPut>? Colors { get; set; }

    public static HueLightSignalingPut NoSignal(int durationMs)
    {
        return new HueLightSignalingPut()
        {
            Signal = HueLightSignal.NoSignal,
            DurationMs = durationMs
        };
    }

    public static HueLightSignalingPut OnOff(int durationMs)
    {
        return new HueLightSignalingPut()
        {
            Signal = HueLightSignal.OnOff,
            DurationMs = durationMs
        };
    }

    public static HueLightSignalingPut OnOffColor(int durationMs, HueXY color)
    {
        return new HueLightSignalingPut()
        {
            Signal = HueLightSignal.OnOffColor,
            DurationMs = durationMs,
            Colors = [new ColorFeatureBasicPut(color)]
        };
    }

    public static HueLightSignalingPut Alternating(int durationMs, HueXY color1, HueXY color2)
    {
        return new HueLightSignalingPut()
        {
            Signal = HueLightSignal.Alternating,
            DurationMs = durationMs,
            Colors = [new ColorFeatureBasicPut(color1), new ColorFeatureBasicPut(color2)]
        };
    }
}
