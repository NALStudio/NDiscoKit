using System.Runtime.CompilerServices;

namespace NDiscoKit.Lights.Models.Color;

public readonly partial struct NDKColor : IEquatable<NDKColor>
{
    public double X { get; }
    public double Y { get; }
    public double Brightness { get; }

    public NDKColor(double x, double y, double brightness)
    {
        // I had a crash because we tried to send NaN to Philips Hue
        // Therefore I added these checks so that if it ever happens again,
        // I can catch it and actually diagnose the issue
        // since it was impossible to diagnose from the Philips Hue request
        ThrowIfNotFinite(x);
        ThrowIfNotFinite(y);
        ThrowIfNotFinite(brightness);

        // I could also check if values are in range, but I think that's a bit unnecessary
        // as the X and Y can in theory be any value since the CIE1931 color space doesn't limit its values into any specific range
        // Brightness on the other hand, is something we control and should be between 0 and 1.
        // I decided to not check this as well just to be consistent with XY checking.

        X = x;
        Y = y;
        Brightness = brightness;
    }

    public NDKColor CopyWith(double? x = null, double? y = null, double? brightness = null)
        => new(x ?? X, y ?? Y, brightness ?? Brightness);

    public override bool Equals(object? obj)
    {
        return obj is NDKColor color && Equals(color);
    }

    public bool Equals(NDKColor other)
    {
        return X == other.X &&
               Y == other.Y &&
               Brightness == other.Brightness;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Brightness);
    }

    public static bool operator ==(NDKColor left, NDKColor right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NDKColor left, NDKColor right)
    {
        return !(left == right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowIfNotFinite(double value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(paramName, $"Value must be finite, not: '{value}'");
    }
}