using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Models.Authentication;
internal class AuthenticationRequest
{
    [JsonPropertyName("devicetype")]
    public required string DeviceType { get; init; }

    [JsonPropertyName("generateclientkey")]
    public bool GenerateClientKey { get; } = true;
}
