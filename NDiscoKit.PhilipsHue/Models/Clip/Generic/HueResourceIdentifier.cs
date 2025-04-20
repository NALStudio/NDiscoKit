using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Models.Clip.Generic;

public sealed class HueResourceIdentifier
{
    [JsonPropertyName("rid")]
    public required Guid ResourceId { get; init; }

    [JsonPropertyName("rtype")]
    public required HueResourceType ResourceType { get; init; }
}
