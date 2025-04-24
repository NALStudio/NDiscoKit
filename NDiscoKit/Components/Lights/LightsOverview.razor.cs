using Microsoft.AspNetCore.Components;
using MudBlazor;
using NDiscoKit.CustomIcons;
using NDiscoKit.Lights.Handlers;
using NDiscoKit.Lights.Handlers.Hue;
using NDiscoKit.Services;
using System.Buffers;
using System.Collections.Immutable;

namespace NDiscoKit.Components.Lights;
public partial class LightsOverview : IDisposable
{
    [Inject]
    private ISnackbar Snackbar { get; init; } = default!;

    [Inject]
    private DiscoLightService LightService { get; init; } = default!;

    private record TreeData(string Name, string? Id = null, LightHandler? Handler = null, Light? Light = null);

    private bool lightHandlersLoading;
    private ImmutableArray<LightHandler>? lightHandlers;

    protected override void OnInitialized()
    {
        LightService.OnHandlersChanged += LightsChanged;
    }

    public void Dispose()
    {
        LightService.OnHandlersChanged -= LightsChanged;
        GC.SuppressFinalize(this);
    }

    private void LightsChanged()
    {
        lightHandlers = null;
        StateHasChanged();
    }

    private async Task LoadLightsAsync()
    {
        lightHandlersLoading = true;

        await Task.Delay(500);
        lightHandlers = await LightService.GetHandlersAsync();

        lightHandlersLoading = false;
        StateHasChanged();
    }

    private async Task SignalLightAsync(Light light)
    {
        bool success;
        if (light.CanIdentify)
        {
            try
            {
                success = await light.IdentifyAsync();
            }
            catch
            {
                success = false;
            }
        }
        else
        {
            success = false;
        }

        if (!success)
            Snackbar.Add("Failed to signal light.", Severity.Error);
    }

    private static string? GetLightIcon(Light light)
    {
        if (light is HueLight hl && hl.LightArchetype is not null)
        {
            // Hue icons use 'bulb-spot' style and archetype uses 'spot_bulb' style.
            // So we have to invert the string and replace dashes with underscores

            ReadOnlySpan<char> archetype = hl.LightArchetype.AsSpan();
            int length = archetype.Length;

            char[]? rented = null;
            Span<char> key = length < 128 ? stackalloc char[length] : (rented = ArrayPool<char>.Shared.Rent(length)).AsSpan(length);
            try
            {
                foreach (Range range in archetype.Split('_'))
                {
                    (int off, int len) = range.GetOffsetAndLength(length);
                    int endIndex = length - off;
                    archetype.Slice(off, len).CopyTo(key[(endIndex - len)..]);
                    if (endIndex < length)
                        key[endIndex] = '-';
                }

                return HueIcons.GetByName(key);
            }
            finally
            {
                if (rented is not null)
                {
                    key.Clear();
                    ArrayPool<char>.Shared.Return(rented, clearArray: false);
                }
            }
        }

        return null;
    }

    private static string? GetHandlerIcon(LightHandler handler)
    {
        return handler switch
        {
            HueLightHandler => HueIcons.BridgeV2,
            _ => null
        };
    }
}
