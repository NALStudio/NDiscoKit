using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Models.Clip.Internal;

internal class HueError
{
    public string Description { get; }

    [JsonConstructor]
    internal HueError(string description)
    {
        Description = description;
    }
}