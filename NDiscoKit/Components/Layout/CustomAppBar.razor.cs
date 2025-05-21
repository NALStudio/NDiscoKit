using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using NDiscoKit.CustomIcons;
using NDiscoKit.Models;
using NDiscoKit.Models.Settings;
using NDiscoKit.Services;
using System.Diagnostics;

namespace NDiscoKit.Components.Layout;
public partial class CustomAppBar : IDisposable
{
    [Inject]
    private ILogger<CustomAppBar> Logger { get; init; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; init; } = default!;

    [Inject]
    private SettingsService SettingsService { get; init; } = default!;

    [Inject]
    private AudioRecordingService AudioService { get; init; } = default!;

    [Inject]
    private DiscoService DiscoService { get; init; } = default!;

    [Parameter, EditorRequired]
    public EventCallback ToggleDrawer { get; set; }

    private readonly Dictionary<AudioSource, bool> sourceFoundCache = new();

    protected override async Task OnInitializedAsync()
    {
        DiscoService.RunningChanged += DiscoService_RunningChanged;
        AudioService.SourceChanged += AudioService_SourceChanged;

        Settings settings = await SettingsService.GetSettingsAsync();

        if (settings.DiscoEnabled)
        {
            await SetDiscoRunning(settings.DiscoEnabled, updateSettings: false);
            StateHasChanged();
        }

        // This is by far the slowest method, so run this last
        if (settings.Source.HasValue)
        {
            AudioSource source = settings.Source.Value;

            // Only try to set source if we can find the process during startup
            if (AudioService.TryFindProcess(source, out Process? p))
            {
                p.Dispose();
                await SetAudioSource(source, updateSettings: false);
                StateHasChanged();
            }
        }
    }

    private void AudioService_SourceChanged(object? sender, AudioSource? e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void DiscoService_RunningChanged(object? sender, DiscoService.RunningChangedEventArgs e)
    {
        if (e.Error is not null)
            Snackbar.Add("The disco service encountered an error.", Severity.Error);
        InvokeAsync(StateHasChanged);
    }


    private Task SetDiscoRunning(bool value) => SetDiscoRunning(value, updateSettings: true).AsTask();
    private async ValueTask SetDiscoRunning(bool value, bool updateSettings)
    {
        if (value)
            DiscoService.Start();
        else
            DiscoService.Stop();

        if (updateSettings)
            await SettingsService.UpdateSettingsAsync(s => s with { DiscoEnabled = value });
    }

    private async ValueTask SetAudioSource(AudioSource? value, bool updateSettings)
    {
        try
        {
            if (value.HasValue)
                await AudioService.StartRecordAsync(value.Value);
            else
                await AudioService.StopRecordAsync();

            if (updateSettings)
                await SettingsService.UpdateSettingsAsync(s => s with { Source = value });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to set audio source.");
            Snackbar.Add("Failed to set audio source.", Severity.Error);
        }
    }

    private static string? GetAudioSourceIcon(AudioSource? source, bool inheritColor = false)
    {
        return source switch
        {
            AudioSource.Spotify => inheritColor ? BrandIcons.Spotify : BrandIcons.SpotifyGreen,
            AudioSource.WindowsMediaPlayer => BrandIcons.WindowsMediaPlayer,
            null => Icons.Material.Rounded.MicOff,
            _ => null
        };
    }

    private static Color GetAudioSourceColor(AudioSource? source)
    {
        return source switch
        {
            AudioSource.Spotify => Color.Success,
            AudioSource.WindowsMediaPlayer => Color.Info,
            null => Color.Dark,
            _ => Color.Error
        };
    }

    private static string GetAudioSourceLabel(AudioSource? source)
    {
        return source switch
        {
            AudioSource.Spotify => "Spotify",
            AudioSource.WindowsMediaPlayer => "Media Player",
            null => "None",
            _ => "Unknown"
        };
    }

    private void AudioSourceMenuOpened(bool opened)
    {
        if (opened)
            sourceFoundCache.Clear();
    }

    private bool SourceFound(AudioSource source, bool cached)
    {
        if (cached && sourceFoundCache.TryGetValue(source, out bool cachedFound))
            return cachedFound;

        bool found = SourceFound(source);
        if (cached)
            sourceFoundCache[source] = found;

        return found;
    }

    private bool SourceFound(AudioSource source)
    {
        if (AudioService.TryFindProcess(source, out Process? p))
        {
            p.Dispose();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Dispose()
    {
        DiscoService.RunningChanged -= DiscoService_RunningChanged;
        AudioService.SourceChanged -= AudioService_SourceChanged;
        GC.SuppressFinalize(this);
    }
}
