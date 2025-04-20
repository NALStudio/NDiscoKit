using NDiscoKit.PhilipsHue.Enums.Clip;
using NDiscoKit.PhilipsHue.Models.Clip.Generic;
using NDiscoKit.PhilipsHue.Models.Clip.Shared;

namespace NDiscoKit.PhilipsHue.Models.Clip.Put;
public sealed class HueEntertainmentConfigurationPut : HueResourcePut
{
    public HueMetadata? Metadata { get; set; }
    public HueEntertainmentConfigurationType? Type { get; set; }
    public HueAction? Action { get; set; }
    public HueEntertainmentStreamProxyPut? StreamProxy { get; set; }
    public HueLocations<HueServiceLocationPut>? Locations { get; set; }
}
