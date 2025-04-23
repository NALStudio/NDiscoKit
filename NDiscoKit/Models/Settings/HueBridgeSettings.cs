using NDiscoKit.PhilipsHue.Models;

namespace NDiscoKit.Models.Settings;
public record HueBridgeSettings
{
    public required string BridgeIp { get; init; }
    public required HueCredentials Credentials { get; init; }
    public Guid? EntertainmentAreaId { get; init; }
}
