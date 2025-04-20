using NDiscoKit.PhilipsHue.Models.Clip.Generic;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueLightGet : HueResourceGet
{
    public required HueResourceIdentifier Owner { get; init; }
    public required HueLightMetadataGet Metadata { get; init; }
    public HueLightProductDataGet? ProductData { get; init; }

    // identify is for PUT

    public int ServiceId { get; init; }
    public required HueLightOn On { get; init; }
    public HueLightDimmingGet? Dimming { get; init; }

    // dimming_delta is for PUT

    public HueLightColorTemperatureGet? ColorTemperature { get; init; }
    public HueLightColorGet? Color { get; init; }

    // Rest are skipped because I don't need them right now
}
