using NDiscoKit.PhilipsHue.Models.Clip.Generic;

namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueEntertainmentServiceGet : HueResourceGet
{
    public required HueResourceIdentifier Owner { get; init; }
    public required bool Renderer { get; init; }
    public HueResourceIdentifier? RendererReference { get; init; }
    public required bool Proxy { get; init; }
    public required bool Equalizer { get; init; }
    public int MaxStreams { get; init; }
    public HueEntertainmentSegmentsGet? Segments { get; init; }
}
