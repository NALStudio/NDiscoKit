using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace NDiscoKit.Components.Disco;
public partial class DiscoButton
{
    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public Color Color { get; set; } = Color.Primary;

    [Parameter]
    /// <summary>
    /// Should the button be held or toggled.
    /// </summary>
    public bool Toggle { get; set; }

    [Parameter]
    public string Tooltip { get; set; } = "Unknown Effect";

    [Parameter]
    public bool State { get; set; }

    [Parameter]
    public EventCallback<bool> StateChanged { get; set; }

    private async Task OnPointerDown()
    {
        if (!Toggle)
            await SetState(true);
    }

    private async Task OnPointerUp()
    {
        if (!Toggle)
            await SetState(false);
    }

    private async Task OnClick()
    {
        if (Toggle)
            await SetState(!State);
    }

    private async Task SetState(bool state)
    {
        State = state;
        await StateChanged.InvokeAsync(state);
    }
}
