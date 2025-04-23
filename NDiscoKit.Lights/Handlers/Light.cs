using NDiscoKit.Lights.Models;
using NDiscoKit.Lights.Models.Color;

namespace NDiscoKit.Lights.Handlers;

public abstract class Light
{
    public required string? DisplayName { get; init; }
    public required LightPosition Position { get; init; }
    public required ColorGamut? ColorGamut { get; init; }
    public required TimeSpan? ExpectedLatency { get; init; }

    public NDKColor Color { get; set; } = NDKColor.WhitePoints.D65;

    public abstract bool CanSignal { get; }

    /// <summary>
    /// Signal for <paramref name="duration"/> using <see cref="Color" />
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the signal was successful, <see langword="false"/> otherwise.
    /// </returns>
    public ValueTask<bool> Signal(TimeSpan duration) => Signal(duration, Color);

    /// <summary>
    /// Signal for <paramref name="duration"/> using the given color.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the signal was successful, <see langword="false"/> otherwise.
    /// </returns>
    public abstract ValueTask<bool> Signal(TimeSpan duration, NDKColor color);
}