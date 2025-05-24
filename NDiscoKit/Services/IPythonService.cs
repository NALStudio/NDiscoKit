using NDiscoKit.Python;

namespace NDiscoKit.Services;
public interface IPythonService
{
    ValueTask<NDKPython> GetPythonAsync();
}