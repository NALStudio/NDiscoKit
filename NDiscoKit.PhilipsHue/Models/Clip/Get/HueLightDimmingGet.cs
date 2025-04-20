namespace NDiscoKit.PhilipsHue.Models.Clip.Get;
public class HueLightDimmingGet
{
    public required double Brightness { get; init; }
    public double MinDimLevel { get; init; }
}
