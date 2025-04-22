using NDiscoKit.Models.Settings;
using System.Text.Json;

namespace NDiscoKit.Services;
public class SettingsService
{
    private const string _kSettingsKey = "settings";

    private readonly IAppDataService appData;
    private readonly JsonSerializerOptions serializerOptions;
    public SettingsService(IAppDataService appData)
    {
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
            return new ValueTask<Settings>(GetOrLoadSettingsInsideLockAsync());
    }

    private async Task<Settings> GetOrLoadSettingsInsideLockAsync()
    {
        await _settingsLock.WaitAsync();
        try
        {
            // The settings value might have been set while we were waiting for the settings lock
            if (_settings is not null)
                return _settings;

            // Load settings
            Settings? s = await appData.GetAsync<Settings>(_kSettingsKey, serializerOptions);
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
            _settingsLock.Release();
        }
    }

    public async Task UpdateSettings(Func<Settings, Settings> updateFunc)
    {
        Settings s = await GetSettingsAsync();
        s = updateFunc(s);
        _settings = s;
        await appData.SetAsync("settings", s, serializerOptions);

        OnSettingsChanged?.Invoke(s);
    }
}
