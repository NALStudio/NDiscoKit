using System.Collections.Immutable;

namespace NDiscoKit.PhilipsHue.Models.Clip.Internal;

internal sealed class HueResponse<T>
{
    public required ImmutableArray<HueError> Errors { get; init; }
    public required ImmutableArray<T> Data { get; init; }
}