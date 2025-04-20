namespace NDiscoKit.PhilipsHue;

internal static class HueEndpoints
{
    public static Uri BaseAddress(string bridgeIp, string? api = null) => new($"https://{bridgeIp}{api}");

    public const string AuthenticationV1 = "/api";
    public const string ClipV2 = "/clip/v2";

    public static class Clip
    {
        public static string WithId(string endpoint, Guid id) => endpoint + "/" + id.ToString();

        public const string Light = "/resource/light";
        public const string EntertainmentConfiguration = "/resource/entertainment_configuration";
        public const string EntertainmentService = "/resource/entertainment";
    }
}