﻿using NDiscoKit.Lights.Models.Color;
using System.Globalization;

namespace NDiscoKit.Lights.Helpers;
public static class ColorHelpers
{
    /// <summary>
    /// Arguments are clamped to the range 0-1.
    /// </summary>
    public static string ToHTMLColorRGBA(double r, double g, double b, double a)
    {
        byte red = BitResolution.AsUInt8(r);
        byte green = BitResolution.AsUInt8(g);
        byte blue = BitResolution.AsUInt8(b);
        byte alpha = BitResolution.AsUInt8(a);

        return $"#{red:x2}{green:x2}{blue:x2}{alpha:x2}";
    }
    /// <inheritdoc cref="ToHTMLColorRGBA(double, double, double, double)"/>
    public static string ToHTMLColorRGBA((double R, double G, double B, double A) rgba)
        => ToHTMLColorRGBA(rgba.R, rgba.G, rgba.B, rgba.A);

    /// <summary>
    /// Arguments are clamped to the range 0-1.
    /// </summary>
    public static string ToHTMLColorRGB(double r, double g, double b)
    {
        byte red = BitResolution.AsUInt8(r);
        byte green = BitResolution.AsUInt8(g);
        byte blue = BitResolution.AsUInt8(b);

        return $"#{red:x2}{green:x2}{blue:x2}";
    }
    /// <inheritdoc cref="ToHTMLColorRGB(double, double, double)"/>
    public static string ToHTMLColorRGB((double R, double G, double B) rgb)
        => ToHTMLColorRGB(rgb.R, rgb.G, rgb.B);

    public static string ToHTMLColorXYZ(double x, double y, double z)
    {
        return string.Format(CultureInfo.InvariantCulture, "color(xyz {0} {1} {2})", x, y, z);
    }
    public static string ToHTMLColorXYZ(NDKColor color)
    {
        (double x, double y, double z) = color.ToXYZ();
        return ToHTMLColorXYZ(x, y, z);
    }

    // public static RGBColor Lerp(RGBColor a, RGBColor b, double t)
    // {
    //     t = Math.Clamp(t, 0d, 1d);
    // 
    //     return new RGBColor(
    //         a.R + ((b.R - a.R) * t),
    //         a.G + ((b.G - a.G) * t),
    //         a.B + ((b.B - a.B) * t)
    //     );
    // }

    // https://en.wikipedia.org/wiki/SRGB#From_sRGB_to_CIE_XYZ
    // https://www.color.org/sRGB.pdf  (Inverting the color component transfer function), there is a typo (12,02 should be 12,92)
    public static double SRGBInverseCompanding(double c)
    {
        if (c <= 0.04045d)
            return c / 12.92d;
        else
            return Math.Pow((c + 0.055d) / 1.055d, 2.4d);
    }

    // https://en.wikipedia.org/wiki/SRGB#From_CIE_XYZ_to_sRGB
    // https://www.color.org/sRGB.pdf  (Color component transfer function)
    public static double SRGBCompanding(double c)
    {
        if (c <= 0.0031308d)
            return c * 12.92d;
        else
            return (1.055d * Math.Pow(c, 1d / 2.4d)) - 0.055d;
    }
}
