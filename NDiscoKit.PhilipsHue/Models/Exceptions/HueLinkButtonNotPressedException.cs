namespace NDiscoKit.PhilipsHue.Models.Exceptions;
public class HueLinkButtonNotPressedException : HueAuthenticationException
{
    public HueLinkButtonNotPressedException() : base()
    {
    }

    public HueLinkButtonNotPressedException(string? message) : base(message)
    {
    }

    public HueLinkButtonNotPressedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
