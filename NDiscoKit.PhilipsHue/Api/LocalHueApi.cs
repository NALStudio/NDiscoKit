using NDiscoKit.PhilipsHue.Helpers;
using NDiscoKit.PhilipsHue.Models;
using NDiscoKit.PhilipsHue.Models.Clip.Generic;
using NDiscoKit.PhilipsHue.Models.Clip.Internal;
using NDiscoKit.PhilipsHue.Models.Exceptions;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Net.Mime;

namespace NDiscoKit.PhilipsHue.Api;
public class LocalHueApi : HueApi
{
    // Exposed for the entertainment API
    public string BridgeIp { get; }
    public HueCredentials Credentials { get; }

    private readonly HueHttpClient http;

    public LocalHueApi(string bridgeIp, HueCredentials credentials)
    {
        BridgeIp = bridgeIp;
        http = new HueHttpClient(HueEndpoints.BaseAddress(bridgeIp));
        http.DefaultRequestHeaders.Add("hue-application-key", credentials.AppKey);
    }

    public override void Dispose()
    {
        http.Dispose();
        GC.SuppressFinalize(this);
    }

    protected override async Task<ImmutableArray<T>> GetAllAsync<T>(string endpoint, CancellationToken cancellationToken)
    {
        return await InternalGetAsync<T>(HueEndpoints.Clip.GetEndpoint(endpoint), cancellationToken);
    }

    protected override async Task<T> GetAsync<T>(string endpoint, Guid id, CancellationToken cancellationToken)
    {
        ImmutableArray<T> values = await InternalGetAsync<T>(HueEndpoints.Clip.GetEndpoint(endpoint, id), cancellationToken);
        return EnsureSingle(values);
    }

    private async Task<ImmutableArray<T>> InternalGetAsync<T>(string fullEndpoint, CancellationToken cancellationToken)
    {
        HttpResponseMessage resp = await http.GetAsync(fullEndpoint, cancellationToken);
        return await HandleResponse<T>(resp, cancellationToken);
    }

    protected override async Task PutAsync<T>(string endpoint, Guid id, T value, CancellationToken cancellationToken)
    {
        HttpResponseMessage resp = await http.PutAsJsonAsync(HueEndpoints.Clip.GetEndpoint(endpoint, id), value, JsonOptions, cancellationToken);
        ImmutableArray<HueResourceIdentifier> result = await HandleResponse<HueResourceIdentifier>(resp, cancellationToken);
        Debug.Assert(result.Length == 1 && result[0].ResourceId == id);
    }

    private static T EnsureSingle<T>(ImmutableArray<T> values)
    {
        if (values.Length != 1)
            throw HueRequestException.UnexpectedResourceCount(values.Length, expectedCount: 1);
        return values[0];
    }

    private async Task<ImmutableArray<T>> HandleResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        HueResponse<T>? resp;
        if (response.Content.Headers.ContentType?.MediaType == MediaTypeNames.Application.Json)
            resp = await response.Content.ReadFromJsonAsync<HueResponse<T>>(JsonOptions, cancellationToken);
        else
            resp = null;

        if (!response.IsSuccessStatusCode)
            throw HueRequestException.FromResponse(response.StatusCode, resp?.Errors);
        if (resp is null)
            throw HueRequestException.CouldNotDeserializeResponse();
        if (resp.Errors.Length > 0)
            throw HueRequestException.FromErrors(resp.Errors);

        return resp.Data!.Value; // Data should not be null if status code is success
    }
}
