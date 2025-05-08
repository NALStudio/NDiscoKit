using Humanizer;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using NDiscoKit.CustomIcons;
using NDiscoKit.Models;
using NDiscoKit.Models.Settings;
using NDiscoKit.Services;
using System.Diagnostics;

namespace NDiscoKit.Layout;
public partial class MainLayout : IDisposable
{
    [Inject]
    private ILogger<MainLayout> Logger { get; init; } = default!;

    [Inject]
    private DiscoService DiscoService { get; init; } = default!;

    [Inject]
    private IAudioRecordingService AudioService { get; init; } = default!;

    [Inject]
    private SettingsService SettingsService { get; init; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private bool drawerOpen = true;

    private bool startingRecord;
    private AudioSource? startingRecordSource;

    private Dictionary<AudioSource, bool> audioSourceFoundCache = new();

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
            await SetAudioSource(settings.Source.Value, updateSettings: false);
            StateHasChanged();
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

    private void ToggleDrawer()
    {
        drawerOpen = !drawerOpen;
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
            {
                startingRecord = true;
                startingRecordSource = value;
                StateHasChanged();

                await AudioService.StartRecordAsync(value.Value);
            }
            else
            {
                await AudioService.StopRecordAsync();
            }

            if (updateSettings)
                await SettingsService.UpdateSettingsAsync(s => s with { Source = value });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to set audio source.");
            Snackbar.Add("Failed to set audio source.", Severity.Error);
        }

        if (startingRecord)
        {
            startingRecord = false;
            StateHasChanged();
        }
    }

    private void AudioSourceMenuOpened(bool opened)
    {
        if (opened)
            audioSourceFoundCache.Clear();
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
        return source?.Humanize(LetterCasing.Title) ?? "None";
    }

    private bool SourceNotFound(AudioSource? source)
    {
        if (!source.HasValue)
            return false;

        if (!audioSourceFoundCache.TryGetValue(source.Value, out bool found))
        {
            Logger.LogInformation("Checking if audio source '{}' is available...", source);

            found = AudioService.TryFindProcess(source.Value, out Process? p);
            if (found)
                p!.Dispose();

            audioSourceFoundCache[source.Value] = found;
        }

        return !found;
    }

    public void Dispose()
    {
        DiscoService.RunningChanged -= DiscoService_RunningChanged;
        AudioService.SourceChanged -= AudioService_SourceChanged;
        GC.SuppressFinalize(this);
    }
}
