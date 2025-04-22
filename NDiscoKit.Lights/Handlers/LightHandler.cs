using System.Collections.Immutable;

namespace NDiscoKit.Lights.Handlers;
public abstract class LightHandler : IAsyncDisposable
{
    public abstract ImmutableArray<Light> Lights { get; }

    /// <summary>
    /// Returns <see langword="true"/> if handler was started successfully, <see langword="false"/> otherwise.
    /// </summary>
    public abstract ValueTask<bool> Start();

    /// <summary>
    /// Update all of the <see cref="Lights"/> with the new colors set.
    /// </summary>
    public abstract ValueTask Update();

    /// <summary>
    /// Should be safe to call multiple times.
    /// </summary>
    public abstract ValueTask Stop();

    public abstract ValueTask DisposeAsync();
}
