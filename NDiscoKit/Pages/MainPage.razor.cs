using Microsoft.AspNetCore.Components;
using NDiscoKit.Models.Settings;
using NDiscoKit.Services;

namespace NDiscoKit.Pages;
public partial class MainPage : IDisposable
{
    [Inject]
    private DiscoService DiscoService { get; init; } = default!;

    [Inject]
    private SettingsService SettingsService { get; init; } = default!;

    private Settings? settings;
    private bool discoDisabled = false;

    protected override async Task OnInitializedAsync()
    {
        DiscoService.RunningChanged += DiscoServiceRunningChanged;
        UpdateDiscoDisabled(discoServiceRunning: DiscoService.IsRunning);

        SettingsService.OnSettingsChanged += OnSettingsChanged;
        settings = await SettingsService.GetSettingsAsync();
        StateHasChanged();
    }

    public void Dispose()
    {
        DiscoService.RunningChanged -= DiscoServiceRunningChanged;
        SettingsService.OnSettingsChanged -= OnSettingsChanged;
        GC.SuppressFinalize(this);
    }

    private void DiscoServiceRunningChanged(object? _, DiscoService.RunningChangedEventArgs args) => InvokeAsync(() => UpdateDiscoDisabled(args.IsRunning));
    private void UpdateDiscoDisabled(bool discoServiceRunning)
    {
        discoDisabled = !discoServiceRunning;
        StateHasChanged();
    }

    private void OnSettingsChanged(Settings? settings)
    {
        this.settings = settings;
        StateHasChanged();
    }

    private async Task SetEffectAuto(bool value)
    {
        await SettingsService.UpdateSettingsAsync(s => s with { AutoEffectEnabled = value });
    }

    private async Task SetFlashAuto(bool value)
    {
        await SettingsService.UpdateSettingsAsync(s => s with { AutoFlashEnabled = value });
    }
}
