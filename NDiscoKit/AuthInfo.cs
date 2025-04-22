namespace NDiscoKit;
internal static class AuthInfo
{
    public const string HueAppName = "NDiscoKit";
    public static string GetHueInstanceName()
    {
        string name = Environment.MachineName;
        if (name.Length > 19)
            name = name[..19];
        return name;
    }
}
