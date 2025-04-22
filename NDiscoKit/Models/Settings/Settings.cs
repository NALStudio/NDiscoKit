using System.Collections.Immutable;

namespace NDiscoKit.Models.Settings;
public record Settings
{
    public ImmutableArray<HueBridgeSettings> HueBridges { get; set; } = ImmutableArray<HueBridgeSettings>.Empty;
}
