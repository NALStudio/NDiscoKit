﻿namespace NDiscoKit.Lights.Models.Color;
public partial struct NDKColor
{
    // https://developers.meethue.com/develop/application-design-guidance/color-conversion-formulas-rgb-to-xy-and-back/#Color-rgb-to-xy
    public static NDKColor FromXYZ(double x, double y, double z)
    {
        double sum = x + y + z;

        return new(
            x: x / sum,
            y: y / sum,
            brightness: y
        );
    }

    // https://developers.meethue.com/develop/application-design-guidance/color-conversion-formulas-rgb-to-xy-and-back/#xy-to-rgb-color
    public (double X, double Y, double Z) ToXYZ()
    {
        double x = this.X;
        double y = this.Y;
        double z = 1d - x - y;

        double Y = Brightness;
        double X = (Y / y) * x;
        double Z = (Y / y) * z;

        return (X, Y, Z);
    }
}
