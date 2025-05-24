using Microsoft.AspNetCore.Components;
using MudBlazor;
using NDiscoKit.AudioAnalysis.Models;
using NDiscoKit.Dialogs;
using NDiscoKit.Services;

namespace NDiscoKit.Components.Disco;
public partial class DiscoSettingsPanel
{
    [Inject]
    private IDialogService DialogService { get; init; } = default!;

    [Inject]
    private AudioTempoService TempoService { get; init; } = default!;

    private bool _autoTempo;
    protected override void OnInitialized()
    {
        TempoService.TempoChanged += TempoService_TempoChanged;
        _autoTempo = TempoService.Auto;
    }

    private void TempoService_TempoChanged(object? sender, AudioTempoService.TempoCollection e)
    {
        InvokeAsync(StateHasChanged);
    }

    private async Task SetAutoAsync(bool auto)
    {
        LoadingDialog.CloseHandle dialog = default;
        try
        {
            ValueTask autoTask = TempoService.SetAutoAsync(auto).Preserve();
            bool taskIsAsync = !autoTask.IsCompleted;

            if (taskIsAsync)
            {
                // Task is asynchronous, show loading dialog
                dialog = await LoadingDialog.ShowAsync(DialogService, "Loading Python...", "This may take several minutes.");
                await autoTask;
            }

            if (_autoTempo != auto)
            {
                _autoTempo = auto;
                if (taskIsAsync)
                    StateHasChanged();
            }
        }
        finally
        {
            if (!dialog.IsDefault)
                dialog.Close();
        }
    }

    private void SetTempo1(double tempo)
    {
        if (TempoService.Auto)
            return;

        TempoService.T1 = new Tempo(tempo);
    }

    private void SetTempo2(double tempo)
    {
        if (TempoService.Auto)
            return;

        TempoService.T2 = new Tempo(tempo);
    }
}
