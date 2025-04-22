using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace NDiscoKit.Services;

/// <summary>
/// Thread-safe methods to store app data.
/// </summary>
public interface IAppDataService
{
    /// <summary>
    /// Store the given value inside <paramref name="key"/>.
    /// </summary>
    abstract ValueTask SetAsync<T>(string key, T value, JsonSerializerOptions? options = null);

    /// <inheritdoc cref="SetAsync" />
    abstract ValueTask SetAsync<T>(string key, T value, JsonTypeInfo<T> jsonTypeInfo);

    /// <summary>
    /// <para>Tries to fetch the value stored in <paramref name="key"/>.</para>
    /// <para>If the given key isn't found, return <see langword="default"/> instead.</para>
    /// </summary>
    abstract ValueTask<T?> GetAsync<T>(string key, JsonSerializerOptions? options = null);

    /// <inheritdoc cref="GetAsync" />
    abstract ValueTask<T?> GetAsync<T>(string key, JsonTypeInfo<T> jsonTypeInfo);

    /// <summary>
    /// Returns <see langword="true"/> if the key was deleted, otherwise (if key didn't exist) <see langword="false"/>.
    /// </summary>
    abstract ValueTask<bool> DeleteAsync(string key);
}
