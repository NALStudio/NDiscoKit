namespace NDiscoKit.Windows;
internal static class Constants
{
    public static string GetAppDataDirectory() => GetAppDataDirectory(null);
    public static string GetAppDataDirectory(string? subdirectory, bool create = false)
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string dirpath = Path.Join(appData, "NALStudio/NDiscoKit", subdirectory);
        if (create)
            Directory.CreateDirectory(dirpath);
        return dirpath;
    }

    public static class AppDataPaths
    {
        public const string WebViewUserDataFolder = "WebView";
    }

    public const string UserAgent = "NDiscoKit/1.0";
}
