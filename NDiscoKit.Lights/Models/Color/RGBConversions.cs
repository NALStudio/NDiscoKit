using NDiscoKit.Lights.Helpers;

namespace NDiscoKit.Lights.Models.Color;
public partial struct NDKColor
{
    #region Linear RGB
    public static NDKColor FromLinearRGB(double r, double g, double b)
    {
        // Not needed as we don't use a gamma function
        // but we check just in case the user assumes that the values range from 0-255 and not 0-1
        // so the user gets an error instead of being puzzled like "wtf? why isn't this working?"
        if (r < 0d || r > 1d)
            throw new ArgumentOutOfRangeException(nameof(r));
        if (g < 0d || g > 1d)
            throw new ArgumentOutOfRangeException(nameof(g));
        if (b < 0d || b > 1d)
            throw new ArgumentOutOfRangeException(nameof(b));

        // The sRGB standard defines four decimals forwards and 7 decimals backwards
        double x = (0.4124d * r) + (0.3576d * g) + (0.1805d * b);
        double y = (0.2126d * r) + (0.7152d * g) + (0.0722d * b);
        double z = (0.0193d * r) + (0.1192d * g) + (0.9505d * b);

        return FromXYZ(x, y, z);
    }

    // https://en.wikipedia.org/wiki/SRGB#From_CIE_XYZ_to_sRGB
    // https://www.color.org/chardata/rgb/sRGB.pdf
    public (double R, double G, double B) ToLinearRGB()
    {
        (double x, double y, double z) = ToXYZ();

        double r = (3.2406255d * x) + (-1.537208d * y) + (-0.4986286d * z);
        double g = (-0.9689307d * x) + (1.8757561d * y) + (0.0415175d * z);
        double b = (0.0557101d * x) + (-0.2040211d * y) + (1.0569959d * z);

        return (r, g, b);
    }
    #endregion

    #region sRGB
    public static NDKColor FromSRGB(double r, double g, double b)
    {
        if (r < 0d || r > 1d)
            throw new ArgumentOutOfRangeException(nameof(r));
        if (g < 0d || g > 1d)
            throw new ArgumentOutOfRangeException(nameof(g));
        if (b < 0d || b > 1d)
            throw new ArgumentOutOfRangeException(nameof(b));

        r = ColorHelpers.SRGBInverseCompanding(r);
        g = ColorHelpers.SRGBInverseCompanding(g);
        b = ColorHelpers.SRGBInverseCompanding(b);

        return FromLinearRGB(r, g, b);
    }

    public (double R, double G, double B) ToSRGB()
    {
        (double r, double g, double b) = ToLinearRGB();

        r = ColorHelpers.SRGBCompanding(r);
        g = ColorHelpers.SRGBCompanding(g);
        b = ColorHelpers.SRGBCompanding(b);

        return (r, g, b);
    }
    #endregion
}
