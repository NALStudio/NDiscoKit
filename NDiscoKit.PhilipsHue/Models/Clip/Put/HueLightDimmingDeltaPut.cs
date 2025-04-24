using NDiscoKit.PhilipsHue.Enums.Clip;

namespace NDiscoKit.PhilipsHue.Models.Clip.Put;
public sealed class HueLightDimmingDeltaPut
{
    public required HueDeltaAction Action { get; set; }
    public double? BrightnessDelta { get; set; }
}
