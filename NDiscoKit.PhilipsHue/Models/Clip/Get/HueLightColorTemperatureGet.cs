namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueLightColorTemperatureGet
{
    public required int? Mirek { get; init; }
    public required bool MirekValid { get; init; }
    public required HueMirekSchemaGet MirekSchema { get; init; }
}
