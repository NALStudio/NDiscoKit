namespace NDiscoKit.PhilipsHue.Models.Exceptions;
internal class HueEntertainmentInternalException : HueEntertainmentException
{
    public HueEntertainmentInternalException() : base()
    {
    }

    public HueEntertainmentInternalException(string? message) : base(message)
    {
    }

    public HueEntertainmentInternalException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
