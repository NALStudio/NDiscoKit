using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using NDiscoKit.Dialogs;
using NDiscoKit.Models.Settings;
using NDiscoKit.PhilipsHue.Api;
using NDiscoKit.PhilipsHue.Models;
using NDiscoKit.PhilipsHue.Models.Clip.Get;
using NDiscoKit.Services;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NDiscoKit.Components.Lights;
public partial class HueControls : IDisposable
{
    private readonly record struct BridgeData
    {
        public required HueBridgeSettings Settings { get; init; }

        [MemberNotNullWhen(false, nameof(EntertainmentAreas))]
        public bool CouldNotConnect => EntertainmentAreas is null;
        public required ImmutableDictionary<Guid, HueEntertainmentConfigurationGet>? EntertainmentAreas { get; init; }

        [return: NotNullIfNotNull(nameof(id))]
        public string? GetEntertainmentAreaSelectDisplayName(Guid? id)
        {
            if (!id.HasValue)
                return null;

            if (EntertainmentAreas?.TryGetValue(id.Value, out HueEntertainmentConfigurationGet? ent) == true)
                return ent.Metadata.Name;
            else
                return $"Not Found. ({id})";
        }
    }

    [Inject]
    private ILogger<HueControls> Logger { get; init; } = default!;

    [Inject]
    private SettingsService Settings { get; init; } = default!;

    [Inject]
    private IDialogService DialogService { get; init; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; init; } = default!;

    private (Task<ImmutableArray<BridgeData>> Task, CancellationTokenSource Cancel)? bridgesTask;

    protected override async Task OnInitializedAsync()
    {
        Settings.OnSettingsChanged += SettingsChanged;
        SettingsChanged(await Settings.GetSettingsAsync());
    }

    private void SettingsChanged(Settings obj)
    {
        bridgesTask?.Cancel.Cancel();

        CancellationTokenSource cancel = new();
        Task<ImmutableArray<BridgeData>> task = LoadBridgesAsync(obj.HueBridges, cancel.Token);
        bridgesTask = (task, cancel);
        task.ContinueWith(_ => StateHasChanged(), TaskContinuationOptions.ExecuteSynchronously);

        StateHasChanged();
    }

    private async Task<ImmutableArray<BridgeData>> LoadBridgesAsync(ImmutableArray<HueBridgeSettings> bridges, CancellationToken cancellationToken)
    {
        ImmutableArray<BridgeData>.Builder bridgeBuilder = ImmutableArray.CreateBuilder<BridgeData>(initialCapacity: bridges.Length);

        foreach (HueBridgeSettings settings in bridges)
        {
            // Throttle the requests a bit
            await Task.Delay(500, cancellationToken);

            using LocalHueApi hue = new(settings.BridgeIp, new HueCredentials(settings.AppKey, settings.ClientKey));

            ImmutableArray<HueEntertainmentConfigurationGet>? entertainmentAreas;
            try
            {
                entertainmentAreas = await hue.GetEntertainmentConfigurationsAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Could not connect to bridge.");
                entertainmentAreas = null;
            }

            bridgeBuilder.Add(new BridgeData()
            {
                Settings = settings,
                EntertainmentAreas = entertainmentAreas?.ToImmutableDictionary(key => key.Id)
            });
        }

        return bridgeBuilder.MoveToImmutable();
    }

    public void Dispose()
    {
        Settings.OnSettingsChanged -= SettingsChanged;
        bridgesTask?.Cancel.Cancel();
        GC.SuppressFinalize(this);
    }

    private async Task AddBridge()
    {
        IDialogReference dialog = await DialogService.ShowAsync<AddBridgeDialog>("Add Hue Bridge", new DialogOptions() { FullWidth = true });
        DialogResult? res = await dialog.Result;
        if (res?.Data is HueBridgeSettings bs)
        {
            await Settings.UpdateSettings(s =>
            {
                HueBridgeSettings? oldValue = s.HueBridges.FirstOrDefault(b => b.BridgeIp == bs.BridgeIp);
                if (oldValue is not null)
                {
                    Snackbar.Add($"This bridge exists already: {bs.BridgeIp}");
                    return s with { HueBridges = s.HueBridges.Replace(oldValue, bs) };
                }
                else
                {
                    return s with { HueBridges = s.HueBridges.Add(bs) };
                }
            });
        }
    }

    private async Task RemoveBridge(MudTabPanel panel)
    {
        await Settings.UpdateSettings(s => s with { HueBridges = s.HueBridges.Remove(((BridgeData)panel.ID!).Settings) });
    }
}
