namespace NDiscoKit.Testing;
internal readonly record struct Prediction<T>(double Strength, T Value)
{
    public static implicit operator T(Prediction<T> prediction) => prediction.Value;

    public void Deconstruct(out double strength, out T value)
    {
        strength = Strength;
        value = Value;
    }
}
