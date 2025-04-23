using Microsoft.AspNetCore.Components;
using MudBlazor;
using NDiscoKit.Models.Settings;
using NDiscoKit.PhilipsHue.Api;
using NDiscoKit.PhilipsHue.Models;
using NDiscoKit.PhilipsHue.Models.Exceptions;

namespace NDiscoKit.Dialogs;
public partial class AddBridgeDialog : IDisposable
{
    [Inject]
    private ISnackbar Snackbar { get; init; } = default!;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; init; } = default!;

    private bool IsDiscoveringBridges => bridgeDiscoveryCancel is not null;
    private bool IsAuthenticating => authenticationCancel is not null;

    private const int _kMaxAuthTries = 7;
    private const int _kMillisecondsBetweenAuthTries = 3000;

    private double authenticationProgress;

    private CancellationTokenSource? bridgeDiscoveryCancel;
    private CancellationTokenSource? authenticationCancel;

    private readonly SortedList<string, DiscoveredBridge> bridges = new();

    protected override async Task OnInitializedAsync()
    {
        await DiscoverBridges();
    }

    private async Task ChooseAndAuthenticateBridge(DiscoveredBridge bridge)
    {
        bridgeDiscoveryCancel?.Cancel();

        authenticationCancel = new();
        HueCredentials? credentials;
        try
        {
            credentials = await TryAuthenticate(bridge, authenticationCancel.Token);
        }
        catch (OperationCanceledException)
        {
            credentials = null;
        }
        catch (Exception)
        {
            credentials = null;

            Snackbar.Add("An unexpected error occured while authenticating.", Severity.Error);
            MudDialog.Cancel();
        }

        if (credentials.HasValue)
        {
            HueBridgeSettings settings = new()
            {
                BridgeIp = bridge.IpAddress,
                Credentials = credentials.Value
            };
            MudDialog.Close(settings);
        }
        else
        {
            authenticationCancel = null;
        }
    }

    private async Task<HueCredentials?> TryAuthenticate(DiscoveredBridge bridge, CancellationToken cancellationToken)
    {
        using HueAuthentication auth = new(bridge.IpAddress, bridge.BridgeId);

        for (int i = 0; i < _kMaxAuthTries; i++)
        {
            authenticationProgress = (i + 1) / (double)_kMaxAuthTries;
            StateHasChanged();

            await Task.Delay(_kMillisecondsBetweenAuthTries, cancellationToken);
            try
            {
                return await auth.AuthenticateAsync(AuthInfo.HueAppName, AuthInfo.GetHueInstanceName(), cancellationToken);
            }
            catch (HueLinkButtonNotPressedException)
            {
            }
        }

        return null;
    }

    private async Task DiscoverBridges()
    {
        if (bridgeDiscoveryCancel is not null)
            return;

        bridges.Clear();
        bridgeDiscoveryCancel = new CancellationTokenSource();

        IEnumerable<DiscoveredBridge>? discovered;
        try
        {
            discovered = await HueBridgeDiscovery.Multicast(5000, OnBridgeDiscovered: OnBridgeDiscovered, cancellationToken: bridgeDiscoveryCancel.Token);
        }
        catch (OperationCanceledException)
        {
            discovered = null;
        }

        if (discovered is not null)
        {
            bridges.Clear();
            foreach (DiscoveredBridge b in discovered)
                bridges.Add(b.IpAddress, b);
        }

        bridgeDiscoveryCancel = null;
        StateHasChanged();
    }

    private async void OnBridgeDiscovered(DiscoveredBridge bridge)
    {
        await InvokeAsync(() => OnBridgeDiscoveredMainThreadFunc(bridge));
    }

    private void OnBridgeDiscoveredMainThreadFunc(DiscoveredBridge bridge)
    {
        bridges.Add(bridge.IpAddress, bridge);
        StateHasChanged();
    }

    public void Dispose()
    {
        bridgeDiscoveryCancel?.Cancel();
        authenticationCancel?.Cancel();
        GC.SuppressFinalize(this);
    }
}
