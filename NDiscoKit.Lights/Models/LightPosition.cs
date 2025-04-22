namespace NDiscoKit.Lights.Models;
public readonly struct LightPosition
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public LightPosition(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}