using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace NDiscoKit.Models.Settings;
public record Settings
{
    public ImmutableArray<HueBridgeSettings> HueBridges { get; set; } = ImmutableArray<HueBridgeSettings>.Empty;

    public bool DiscoEnabled { get; set; }

    public bool AutoTempoEnabled { get; set; }
    public bool AutoEffectEnabled { get; set; }
    public bool AutoFlashEnabled { get; set; }

    public int? PythonDependenciesVersion { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter<AudioSource>))]
    public AudioSource? Source { get; set; }
}
