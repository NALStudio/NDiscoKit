using Microsoft.AspNetCore.Components;

namespace NDiscoKit.Components.Layout;
public partial class CustomExpansionPanel
{
    [Parameter]
    public string? Class { get; set; }
    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public string? Title { get; set; }
    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public bool BoldTitle { get; set; }

    [Parameter]
    public bool Expanded { get; set; }
    [Parameter]
    public EventCallback<bool> ExpandedChanged { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
