using NDiscoKit.PhilipsHue.Enums.Clip;
using NDiscoKit.PhilipsHue.Models.Clip.Generic;
using NDiscoKit.PhilipsHue.Models.Clip.Shared;
using System.Collections.Immutable;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueEntertainmentConfigurationGet : HueResourceGet
{
    public required HueMetadata Metadata { get; init; }
    public required HueEntertainmentConfigurationType ConfigurationType { get; init; }
    public required HueStatus Status { get; init; }
    public HueResourceIdentifier? ActiveStreamer { get; init; }
    public required HueEntertainmentStreamProxyGet StreamProxy { get; init; }
    public required ImmutableArray<HueEntertainmentChannelGet> Channels { get; init; }
    public required HueLocations<HueServiceLocationGet> Locations { get; init; }
}
