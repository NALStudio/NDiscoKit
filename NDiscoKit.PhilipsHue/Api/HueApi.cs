using NDiscoKit.PhilipsHue.Models.Clip.Get;
using NDiscoKit.PhilipsHue.Models.Clip.Put;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Api;
public abstract class HueApi : IDisposable
{
    #region Public Methods
    public Task<ImmutableArray<HueLightGet>> GetLights() => GetAll<HueLightGet>(HueEndpoints.Clip.Light);
    public Task<HueLightGet> GetLight(Guid id) => Get<HueLightGet>(HueEndpoints.Clip.Light, id);

    public Task<ImmutableArray<HueEntertainmentConfigurationGet>> GetEntertainmentConfigurations() => GetAll<HueEntertainmentConfigurationGet>(HueEndpoints.Clip.EntertainmentConfiguration);
    public Task<HueEntertainmentConfigurationGet> GetEntertainmentConfiguration(Guid id) => Get<HueEntertainmentConfigurationGet>(HueEndpoints.Clip.EntertainmentConfiguration, id);
    public Task UpdateEntertainmentConfiguration(Guid id, HueEntertainmentConfigurationPut value) => Put(HueEndpoints.Clip.EntertainmentConfiguration, id, value);
    #endregion

    #region Abstract Methods
    protected abstract Task<ImmutableArray<T>> GetAll<T>(string endpoint) where T : HueResourceGet;
    protected abstract Task<T> Get<T>(string endpoint, Guid id) where T : HueResourceGet;
    protected abstract Task Put<T>(string endpoint, Guid id, T value) where T : HueResourcePut;

    public abstract void Dispose();
    #endregion

    #region Json
    public static readonly JsonSerializerOptions JsonOptions = GetSerializerOptions(readOnly: true);
    private static JsonSerializerOptions GetSerializerOptions(bool readOnly = false)
    {
        JsonSerializerOptions opt = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        if (readOnly)
            opt.MakeReadOnly();
        return opt;
    }
    #endregion
}
