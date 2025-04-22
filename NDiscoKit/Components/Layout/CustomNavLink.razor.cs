using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace NDiscoKit.Components.Layout;

/// <summary>
/// A white <see cref="MudBlazor.MudNavLink"/>.
/// </summary>
/// <remarks>
/// Set <see cref="MudBlazor.MudNavMenu.Color"/> to <see cref="MudBlazor.Color.Default"/> to display this component correctly.
/// </remarks>
public partial class CustomNavLink
{
    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public NavLinkMatch Match { get; set; } = NavLinkMatch.Prefix;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
