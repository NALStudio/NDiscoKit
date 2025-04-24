using Microsoft.Extensions.Logging;
using NDiscoKit.Lights.Handlers;
using NDiscoKit.Lights.Handlers.Hue;
using NDiscoKit.Models.Settings;
using NDiscoKit.PhilipsHue.Api;
using System.Collections.Immutable;

namespace NDiscoKit.Services;
internal class DiscoLightService : IAsyncDisposable
{
    private readonly ILogger<DiscoLightService> logger;
    private readonly ILogger<LocalHueApi> hueLogger;

    private readonly SettingsService settings;

    private bool handlersUpToDate = false;
    private readonly SemaphoreSlim handlersLock = new(1, 1);
    private ImmutableArray<LightHandler> handlers = ImmutableArray<LightHandler>.Empty;

    public event Action? OnHandlersChanged;

    public DiscoLightService(ILogger<DiscoLightService> logger, ILogger<LocalHueApi> hueLogger, SettingsService settings)
    {
        this.logger = logger;
        this.hueLogger = hueLogger;

        this.settings = settings;

        settings.OnSettingsChanged += SettingsUpdated;
    }

    private void SettingsUpdated(Settings s)
    {
        Interlocked.Exchange(ref handlersUpToDate, false);
        OnHandlersChanged?.Invoke();
    }

    public async ValueTask<ImmutableArray<LightHandler>> GetHandlersAsync()
    {
        await handlersLock.WaitAsync();
        try
        {
            bool upToDate = Interlocked.Exchange(ref handlersUpToDate, true);
            if (!upToDate)
            {
                await DisposeHandlers(acquireLock: false);
                handlers = await LoadHandlers();
            }
            return handlers;
        }
        finally
        {
            handlersLock.Release();
        }
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
                    handler = await HueLightHandler.CreateAsync(new LocalHueApi(bridge.BridgeIp, bridge.Credentials, hueLogger), bridge.EntertainmentAreaId.Value, disposeHue: true);
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

    private async ValueTask DisposeHandlers(bool acquireLock)
    {
        if (acquireLock)
            await handlersLock.WaitAsync();
        try
        {
            foreach (LightHandler h in handlers)
                await h.DisposeAsync();
        }
        finally
        {
            if (acquireLock)
                handlersLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        settings.OnSettingsChanged -= SettingsUpdated;
        await DisposeHandlers(acquireLock: true);
        GC.SuppressFinalize(this);
    }
}
