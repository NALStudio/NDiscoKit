using NDiscoKit.PhilipsHue.Helpers;
using NDiscoKit.PhilipsHue.Models;
using NDiscoKit.PhilipsHue.Models.Clip.Generic;
using NDiscoKit.PhilipsHue.Models.Clip.Internal;
using NDiscoKit.PhilipsHue.Models.Exceptions;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace NDiscoKit.PhilipsHue.Api;
public class LocalHueApi : HueApi
{
    private readonly HueHttpClient http;

    public LocalHueApi(string bridgeIp, HueCredentials credentials)
    {
        http = new HueHttpClient(HueEndpoints.BaseAddress(bridgeIp, HueEndpoints.ClipV2));
        http.DefaultRequestHeaders.Add("hue-application-key", credentials.AppKey);
    }

    public override void Dispose()
    {
        http.Dispose();
        GC.SuppressFinalize(this);
    }


    protected override async Task<ImmutableArray<T>> GetAll<T>(string endpoint)
    {
        HttpResponseMessage resp = await http.GetAsync(endpoint);
        return await HandleResponse<T>(resp);
    }

    protected override async Task<T> Get<T>(string endpoint, Guid id)
    {
        ImmutableArray<T> values = await GetAll<T>(HueEndpoints.Clip.WithId(endpoint, id));
        return EnsureSingle(values);
    }

    protected override async Task Put<T>(string endpoint, Guid id, T value)
    {
        HttpResponseMessage resp = await http.PutAsJsonAsync(HueEndpoints.Clip.WithId(endpoint, id), value, JsonOptions);
        ImmutableArray<HueResourceIdentifier> result = await HandleResponse<HueResourceIdentifier>(resp);
        Debug.Assert(result.Length == 1 && result[0].ResourceId == id);
    }

    private static T EnsureSingle<T>(ImmutableArray<T> values)
    {
        if (values.Length != 1)
            throw HueRequestException.UnexpectedResourceCount(values.Length, expectedCount: 1);
        return values[0];
    }

    private static async Task<ImmutableArray<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        HueResponse<T>? resp;
        try
        {
            resp = await response.Content.ReadFromJsonAsync<HueResponse<T>>(JsonOptions);
        }
        catch (JsonException e)
        {
            string content = await response.Content.ReadAsStringAsync();
            e.Data.Add("Response Content", content);
            throw;
        }

        if (!response.IsSuccessStatusCode)
            throw HueRequestException.FromResponse(response.StatusCode, resp?.Errors);
        if (resp is null)
            throw HueRequestException.CouldNotDeserializeResponse();
        if (resp.Errors.Length > 0)
            throw HueRequestException.FromErrors(resp.Errors);

        return resp.Data;
    }
}
