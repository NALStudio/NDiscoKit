using NDiscoKit.PhilipsHue.Models.Clip.Get;
using NDiscoKit.PhilipsHue.Models.Clip.Put;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Api;
public abstract class HueApi : IDisposable
{
    #region Public Methods
    public Task<ImmutableArray<HueLightGet>> GetLightsAsync(CancellationToken cancellationToken = default) => GetAllAsync<HueLightGet>(HueEndpoints.Clip.Light, cancellationToken);
    public Task<HueLightGet> GetLightAsync(Guid id, CancellationToken cancellationToken = default) => GetAsync<HueLightGet>(HueEndpoints.Clip.Light, id, cancellationToken);
    public Task UpdateLightAsync(Guid id, HueLightPut value, CancellationToken cancellationToken = default) => PutAsync(HueEndpoints.Clip.Light, id, value, cancellationToken);

    public Task<ImmutableArray<HueEntertainmentConfigurationGet>> GetEntertainmentConfigurationsAsync(CancellationToken cancellationToken = default) => GetAllAsync<HueEntertainmentConfigurationGet>(HueEndpoints.Clip.EntertainmentConfiguration, cancellationToken);
    public Task<HueEntertainmentConfigurationGet> GetEntertainmentConfigurationAsync(Guid id, CancellationToken cancellationToken = default) => GetAsync<HueEntertainmentConfigurationGet>(HueEndpoints.Clip.EntertainmentConfiguration, id, cancellationToken);
    public Task UpdateEntertainmentConfigurationAsync(Guid id, HueEntertainmentConfigurationPut value, CancellationToken cancellationToken = default) => PutAsync(HueEndpoints.Clip.EntertainmentConfiguration, id, value, cancellationToken);

    public Task<ImmutableArray<HueEntertainmentServiceGet>> GetEntertainmentServicesAsync(CancellationToken cancellationToken = default) => GetAllAsync<HueEntertainmentServiceGet>(HueEndpoints.Clip.EntertainmentService, cancellationToken);
    public Task<HueEntertainmentServiceGet> GetEntertainmentServiceAsync(Guid id, CancellationToken cancellationToken = default) => GetAsync<HueEntertainmentServiceGet>(HueEndpoints.Clip.EntertainmentService, id, cancellationToken);
    #endregion

    #region Abstract Methods
    protected abstract Task<ImmutableArray<T>> GetAllAsync<T>(string endpoint, CancellationToken cancellationToken) where T : HueResourceGet;
    protected abstract Task<T> GetAsync<T>(string endpoint, Guid id, CancellationToken cancellationToken) where T : HueResourceGet;
    protected abstract Task PutAsync<T>(string endpoint, Guid id, T value, CancellationToken cancellationToken) where T : HueResourcePut;

    public abstract void Dispose();
    #endregion

    #region Json
    /// <summary>
    /// This instance can be modified by the inheriting class if necessary.
    /// </summary>
    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
    #endregion
}
