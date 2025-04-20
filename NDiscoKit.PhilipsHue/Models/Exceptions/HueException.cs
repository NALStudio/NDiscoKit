namespace NDiscoKit.PhilipsHue.Models.Exceptions;
public class HueException : Exception
{
    public HueException() : base()
    {
    }

    public HueException(string? message) : base(message)
    {
    }

    public HueException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
