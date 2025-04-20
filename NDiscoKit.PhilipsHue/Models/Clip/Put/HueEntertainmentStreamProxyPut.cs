using NDiscoKit.PhilipsHue.Enums.Clip;
using NDiscoKit.PhilipsHue.Models.Clip.Generic;

namespace NDiscoKit.PhilipsHue.Models.Clip.Put;
public sealed class HueEntertainmentStreamProxyPut
{
    public static HueEntertainmentStreamProxyPut Auto() => new(HueProxyMode.Auto, node: null);

    // I'm not sure if node can be null if the mode is manual... I guess we'll see.
    public static HueEntertainmentStreamProxyPut Manual() => Manual(null);
    public static HueEntertainmentStreamProxyPut Manual(HueResourceIdentifier? node) => new(HueProxyMode.Manual, node: node);

    private HueEntertainmentStreamProxyPut(HueProxyMode mode, HueResourceIdentifier? node)
    {
        Mode = mode;
        Node = node;
    }

    public HueProxyMode Mode { get; }
    public HueResourceIdentifier? Node { get; }
}
