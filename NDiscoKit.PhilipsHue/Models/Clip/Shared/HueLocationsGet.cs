using System.Collections.Immutable;

namespace NDiscoKit.PhilipsHue.Models.Clip.Shared;
public class HueLocations<T>
{
    public required ImmutableArray<T> ServiceLocations { get; init; }
}
