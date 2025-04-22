using NDiscoKit.Services;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace NDiscoKit.Windows.Services;
public class WindowsAppDataService : IAppDataService
{
    private readonly SemaphoreSlim FileLock = new(1, 1);

    private static string GetPath(string key) => Path.Join(Constants.GetAppDataDirectory(), key + ".json");

    public async ValueTask<bool> DeleteAsync(string key)
    {
        await FileLock.WaitAsync();
        try
        {
            if (File.Exists(key))
            {
                File.Delete(key);
                return true;
            }
            else
            {
                return false;
            }
        }
        finally
        {
            FileLock.Release();
        }
    }

    public async ValueTask<T?> GetAsync<T>(string key, JsonSerializerOptions? options = null)
    {
        await FileLock.WaitAsync();
        try
        {
            await using FileStream fs = OpenRead(key);
            return await JsonSerializer.DeserializeAsync<T>(fs, options);
        }
        catch (FileNotFoundException)
        {
            return default;
        }
        finally
        {
            FileLock.Release();
        }
    }

    public async ValueTask<T?> GetAsync<T>(string key, JsonTypeInfo<T> jsonTypeInfo)
    {
        await FileLock.WaitAsync();
        try
        {
            await using FileStream fs = OpenRead(key);
            return await JsonSerializer.DeserializeAsync(fs, jsonTypeInfo);
        }
        catch (FileNotFoundException)
        {
            return default;
        }
        finally
        {
            FileLock.Release();
        }
    }

    public async ValueTask SetAsync<T>(string key, T value, JsonSerializerOptions? options = null)
    {
        await FileLock.WaitAsync();
        try
        {
            await using FileStream fs = OpenWrite(key);
            await JsonSerializer.SerializeAsync(fs, value, options);
        }
        finally
        {
            FileLock.Release();
        }
    }

    public async ValueTask SetAsync<T>(string key, T value, JsonTypeInfo<T> jsonTypeInfo)
    {
        await FileLock.WaitAsync();
        try
        {
            await using FileStream fs = OpenWrite(key);
            await JsonSerializer.SerializeAsync(fs, value, jsonTypeInfo);
        }
        finally
        {
            FileLock.Release();
        }
    }

    private static FileStream OpenRead(string key) => File.OpenRead(GetPath(key));
    private static FileStream OpenWrite(string key) => File.Open(GetPath(key), FileMode.Create, FileAccess.Write, FileShare.None);
}
