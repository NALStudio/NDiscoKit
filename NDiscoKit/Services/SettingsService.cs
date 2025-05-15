using Microsoft.Extensions.Logging;
using NDiscoKit.Models.Settings;
using System.Text.Json;

namespace NDiscoKit.Services;
public class SettingsService
{
    private const string _kSettingsKey = "settings";

    private readonly ILogger<SettingsService> logger;
    private readonly IAppDataService appData;
    private readonly JsonSerializerOptions serializerOptions;
    public SettingsService(ILogger<SettingsService> logger, IAppDataService appData)
    {
        this.logger = logger;
        this.appData = appData;

        serializerOptions = new()
        {
            WriteIndented = true
        };
    }

    public event Action<Settings>? OnSettingsChanged;

    private readonly SemaphoreSlim _settingsLock = new(1, 1);
    private Settings? _settings;

    public ValueTask<Settings> GetSettingsAsync()
    {
        if (_settings is not null)
            return ValueTask.FromResult(_settings);
        else
            return new ValueTask<Settings>(GetOrLoadSettingsAsync(acquireLock: true));
    }

    private async Task<Settings> GetOrLoadSettingsAsync(bool acquireLock)
    {
        if (acquireLock)
            await _settingsLock.WaitAsync();
        try
        {
            // The settings value might have been set while we were waiting for the settings lock
            if (_settings is not null)
                return _settings;

            // Load settings
            Settings? s;

            try
            {
                s = await appData.GetAsync<Settings>(_kSettingsKey, serializerOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load settings. Loading default settings instead...");
                s = null;
            }

            if (s is null)
            {
                s = new Settings();
                await appData.SetAsync(_kSettingsKey, s, serializerOptions);
            }

            _settings = s;
            return s;
        }
        finally
        {
            if (acquireLock)
                _settingsLock.Release();
        }
    }

    /// <summary>
    /// Updates the current settings with <paramref name="updateFunc"/>.
    /// </summary>
    /// <returns>The updated settings.</returns>
    public async Task<Settings> UpdateSettingsAsync(Func<Settings, Settings> updateFunc)
    {
        await _settingsLock.WaitAsync();

        Settings updated;
        try
        {
            Settings s = await GetOrLoadSettingsAsync(acquireLock: false);
            updated = updateFunc(s);
            _settings = updated;

            // SetAsync inside lock so that the data on disk isn't updated out of order
            await appData.SetAsync("settings", updated, serializerOptions);
        }
        finally
        {
            _settingsLock.Release();
        }


        OnSettingsChanged?.Invoke(updated);
        return updated;
    }
}
