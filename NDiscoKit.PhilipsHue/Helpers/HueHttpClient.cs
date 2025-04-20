using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace NDiscoKit.PhilipsHue.Helpers;
internal class HueHttpClient : HttpClient
{
    /// <summary>
    /// Set <paramref name="bridgeId"/> if you want to verify it during SSL Certificate validation.
    /// </summary>
    public HueHttpClient(Uri baseAddress, string? bridgeId = null) : base(CreateHandler(bridgeId), disposeHandler: true)
    {
        base.BaseAddress = baseAddress;
    }

    public new Uri BaseAddress => base.BaseAddress ?? throw new Exception("Base address should not be null.");

    public static HttpClientHandler CreateHandler(string? bridgeId)
    {
        return new()
        {
            ServerCertificateCustomValidationCallback = ConstructValidateHueCertificateFunc(bridgeId)
        };
    }

    private static Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool> ConstructValidateHueCertificateFunc(string? bridgeId)
    {
        // There doesn't seem to be a consensus on what to search for in a Philips Hue certificate...
        // A rudimentary check is better than no check at all, so:

        return (HttpRequestMessage _, X509Certificate2? certificate, X509Chain? _, SslPolicyErrors _) =>
        {
            return certificate?.IssuerName.Name.Contains("Philips Hue") == true
                && (bridgeId is null || certificate.SubjectName.Name.Contains(bridgeId));
        };
    }
}
