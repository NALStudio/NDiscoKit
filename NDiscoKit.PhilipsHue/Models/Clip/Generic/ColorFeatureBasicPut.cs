namespace NDiscoKit.PhilipsHue.Models.Clip.Generic;
public sealed class ColorFeatureBasicPut
{
    public HueXY? XY { get; set; }

    public ColorFeatureBasicPut()
    {
    }

    public ColorFeatureBasicPut(HueXY? xy)
    {
        XY = xy;
    }
}
