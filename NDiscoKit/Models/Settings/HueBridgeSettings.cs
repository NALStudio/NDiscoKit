namespace NDiscoKit.Models.Settings;
public class HueBridgeSettings
{
    public required string BridgeIp { get; init; }
    public required string AppKey { get; init; }
    public required string ClientKey { get; init; }
    public Guid? EntertainmentAreaId { get; init; }
}
