using NDiscoKit.PhilipsHue.Enums.Clip;

namespace NDiscoKit.PhilipsHue.Models.Clip.Put;
public sealed class HueLightAlertPut
{
    public required HueAlertAction Action { get; set; }
}
