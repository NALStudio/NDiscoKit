namespace NDiscoKit.PhilipsHue.Models.Exceptions;
public class HueEntertainmentException : HueException
{
    public HueEntertainmentException() : base()
    {
    }

    public HueEntertainmentException(string? message) : base(message)
    {
    }

    public HueEntertainmentException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
