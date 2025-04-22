namespace NDiscoKit.PhilipsHue;

internal static class HueEndpoints
{
    public static Uri BaseAddress(string bridgeIp) => new($"https://{bridgeIp}");

    public const string AuthenticationV1 = "/api";
    public const string ClipV2 = "/clip/v2";

    public static class Clip
    {
        public static string GetEndpoint(string endpoint) => ClipV2 + endpoint;
        public static string GetEndpoint(string endpoint, Guid id) => ClipV2 + endpoint + "/" + id;

        public const string Light = "/resource/light";
        public const string EntertainmentConfiguration = "/resource/entertainment_configuration";
        public const string EntertainmentService = "/resource/entertainment";
    }
}