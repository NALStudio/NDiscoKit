namespace NDiscoKit.PhilipsHue.Models.Exceptions;
public class HueAuthenticationException : HueException
{
    public HueAuthenticationException() : base()
    {
    }

    public HueAuthenticationException(string? message) : base(message)
    {
    }

    public HueAuthenticationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
