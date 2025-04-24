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

    public abstract bool CanIdentify { get; }

    /// <summary>
    /// Run an action to identify this light.
    /// </summary>
    public abstract ValueTask<bool> IdentifyAsync();
}