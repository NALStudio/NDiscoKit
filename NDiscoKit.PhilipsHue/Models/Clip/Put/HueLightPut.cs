using NDiscoKit.PhilipsHue.Models.Clip.Generic;
using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Models.Clip.Put;
public sealed class HueLightPut : HueResourcePut
{
    public HueLightMetadataPut? Metadata { get; set; }
    public HueLightOn? On { get; set; }
    public HueLightDimmingPut? Dimming { get; set; }
    public HueLightDimmingDeltaPut? DimmingDelta { get; set; }
    public HueLightColorTemperaturePut? ColorTemperature { get; set; }
    public ColorFeatureBasicPut? Color { get; set; }
    public HueLightDynamicsPut? Dynamics { get; set; }
    public HueLightAlertPut? Alert { get; set; }
    public HueLightSignalingPut? Signaling { get; set; }
    public HueLightGradientPut? Gradient { get; set; }

    [JsonIgnore]
    public bool Identify
    {
        get => _identify is not null;
        set => _identify = value ? HueLightIdentifyPut.IdentifyAction : null;
    }

    [JsonInclude, JsonPropertyName("identify")]
    private HueLightIdentifyPut? _identify;

    // TODO: Effects and Powerup
}
