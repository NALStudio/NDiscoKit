namespace NDiscoKit.Lights.Models.Color;
public partial struct NDKColor
{
    /// <summary>
    /// Different CIE and ISO white points with a brightness of 1.
    /// </summary>
    public static class WhitePoints
    {
        // https://en.wikipedia.org/wiki/Standard_illuminant#D65_values
        public static readonly NDKColor D65 = new(x: 0.31272d, y: 0.32903d, brightness: 1);
    }
}
