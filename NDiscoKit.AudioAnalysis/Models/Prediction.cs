namespace NDiscoKit.AudioAnalysis.Models;

public readonly record struct Prediction<T>(double Strength, T Value)
{
    public static implicit operator T(Prediction<T> prediction) => prediction.Value;
}
