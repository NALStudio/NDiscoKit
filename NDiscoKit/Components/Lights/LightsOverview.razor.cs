using Microsoft.AspNetCore.Components;
using MudBlazor;
using NDiscoKit.CustomIcons;
using NDiscoKit.Lights.Handlers;
using NDiscoKit.Lights.Handlers.Hue;
using NDiscoKit.Services;
using System.Collections.Immutable;

namespace NDiscoKit.Components.Lights;
public partial class LightsOverview
{
    [Inject]
    private DiscoLightService LightService { get; init; } = default!;

    private record TreeData(string Name, string? Id = null, LightHandler? Handler = null, Light? Light = null);

    private MudTreeView<TreeData>? mudTree;

    private static ImmutableArray<TreeItemData<TreeData>> GetInitialData()
    {
        return [
            new()
            {
                Icon = Icons.Material.Rounded.Lightbulb,
                Expandable = true,
                Value = new TreeData("Lights", Id: "lights")
            }
        ];
    }

    private async Task<IReadOnlyCollection<TreeItemData<TreeData>>> LoadData(TreeData item)
    {
        return item.Id switch
        {
            "lights" => await LoadLights(),
            _ => ImmutableArray<TreeItemData<TreeData>>.Empty
        };
    }

    private static string? GetLightIcon(Light light)
    {
        if (light is HueLight hl && hl.LightArchetype is not null)
            return HueIcons.GetByName(hl.LightArchetype);
        else
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

    private async Task<IReadOnlyCollection<TreeItemData<TreeData>>> LoadLights()
    {
        List<TreeItemData<TreeData>> handlers = new();

        foreach (LightHandler handler in await LightService.GetHandlersAsync())
        {
            TreeItemData<TreeData> handlerItem = new()
            {
                Expandable = true,
                Value = new(handler.DisplayName, Handler: handler),
                Icon = GetHandlerIcon(handler),
                Children = new()
            };

            foreach (Light light in handler.Lights)
            {
                handlerItem.Children.Add(new TreeItemData<TreeData>()
                {
                    Expandable = false,
                    Value = new(light.DisplayName ?? "Unknown Light", Light: light),
                    Icon = GetLightIcon(light)
                });
            }

            handlers.Add(handlerItem);
        }

        return handlers;
    }

    private static void OnItemsLoaded(TreeItemData<TreeData> treeItemData, IReadOnlyCollection<TreeItemData<TreeData>> children)
    {
        treeItemData.Children = children?.ToList();
    }
}
