using Microsoft.Extensions.Logging;
using NDiscoKit.Lights.Handlers;
using NDiscoKit.Lights.Handlers.Hue;
using NDiscoKit.Models.Settings;
using NDiscoKit.PhilipsHue.Api;
using System.Collections.Immutable;

namespace NDiscoKit.Services;
internal class DiscoLightService : IDisposable
{
    private readonly ILogger<DiscoLightService> logger;
    private readonly SettingsService settings;

    private Task<ImmutableArray<LightHandler>>? handlers;

    public event Action? OnHandlersChanged;

    public DiscoLightService(ILogger<DiscoLightService> logger, SettingsService settings)
    {
        this.logger = logger;
        this.settings = settings;

        settings.OnSettingsChanged += SettingsUpdated;
    }

    private void SettingsUpdated(Settings s)
    {
        handlers = null;
        OnHandlersChanged?.Invoke();
    }

    public Task<ImmutableArray<LightHandler>> GetHandlersAsync()
    {
        handlers ??= LoadHandlers();
        return handlers;
    }

    private async Task<ImmutableArray<LightHandler>> LoadHandlers()
    {
        Settings settings = await this.settings.GetSettingsAsync();

        List<LightHandler> handlers = new();

        // Load Philips Hue
        foreach (HueBridgeSettings bridge in settings.HueBridges)
        {
            Exception? error = null;
            HueLightHandler? handler = null;
            try
            {
                if (bridge.EntertainmentAreaId.HasValue)
                    handler = await HueLightHandler.CreateAsync(new LocalHueApi(bridge.BridgeIp, bridge.Credentials), bridge.EntertainmentAreaId.Value, disposeHue: true);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (handler is not null)
                handlers.Add(handler);
            else
                logger.LogError(error, "Failed to create Philips Hue handler.");
        }

        return handlers.ToImmutableArray();
    }

    public void Dispose()
    {
        settings.OnSettingsChanged -= SettingsUpdated;
        GC.SuppressFinalize(this);
    }
}
