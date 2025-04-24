namespace NDiscoKit.PhilipsHue.Models.Clip.Put;
public sealed class HueLightIdentifyPut
{
    public static readonly HueLightIdentifyPut IdentifyAction = new("identify");

    public string Action { get; }
    public HueLightIdentifyPut(string action)
    {
        Action = action;
    }
}
