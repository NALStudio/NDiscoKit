using NDiscoKit.PhilipsHue.Models.Clip.Internal;
using System.Net;
using System.Text;

namespace NDiscoKit.PhilipsHue.Models.Exceptions;
internal class HueRequestException : HueException
{
    public HueRequestException() : base()
    {
    }

    public HueRequestException(string? message) : base(message)
    {
    }

    public HueRequestException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public static HueRequestException CouldNotSerializeRequest() => new("Could not serialize request.");
    public static HueRequestException CouldNotDeserializeResponse() => new("Could not deserialize response.");
    public static HueRequestException UnexpectedResourceCount(int gotCount, int? expectedCount = null)
    {
        StringBuilder sb = new("Unexpected resource count.");
        if (expectedCount.HasValue)
            sb.Append($" Expected: {expectedCount.Value},");
        sb.Append($" Got: {gotCount}.");
        return new(sb.ToString());
    }

    public static HueRequestException FromResponse(HttpStatusCode statusCode, IReadOnlyCollection<HueError>? errors)
    {
        StringBuilder sb = new();

        sb.AppendLine($"Hue request failed with status code: {(int)statusCode}.");

        if (errors?.Count > 0)
        {
            sb.Append("The following errors were returned by the API:\n");
            BuildErrors(sb, errors);
        }
        else
        {
            sb.Append("No additional error data was provided by the API.\n");
        }

        sb.AppendLine();

        return new HueRequestException(sb.ToString());
    }

    public static HueRequestException FromErrors(IEnumerable<HueError> errors)
    {
        StringBuilder sb = new("Hue request failed with the following errors:\n");
        BuildErrors(sb, errors);

        return new HueRequestException(sb.ToString());
    }

    private static void BuildErrors(StringBuilder sb, IEnumerable<HueError> errors)
    {
        int i = 0;
        foreach (HueError err in errors)
        {
            sb.Append("  ");
            sb.Append(i);
            sb.Append(": ");
            sb.Append(err.Description);
            sb.AppendLine();

            i++;
        }

        // enumerable was empty
        if (i == 0)
            sb.Append("  ?: No error data available.");
    }
}
