using NDiscoKit.PhilipsHue.Models.Clip.Generic;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueResourceGet
{
    public required Guid Id { get; init; }
    public HueResourceType? Type { get; init; }
}
