namespace NDiscoKit.AudioAnalysis.Models;
public readonly struct Tempo : IEquatable<Tempo>, IComparable<Tempo>
{
    public Tempo(double bpm)
    {
        BPM = bpm;
    }

    public double BPM { get; }

    public double SecondsPerBeat => 60d / BPM;

    public int CompareTo(Tempo other) => BPM.CompareTo(other.BPM);
    public bool Equals(Tempo other) => BPM.Equals(other.BPM);
    public override bool Equals(object? obj) => obj is Tempo t && Equals(t);
    public override int GetHashCode() => BPM.GetHashCode();

    public static bool operator <(Tempo left, Tempo right) => left.CompareTo(right) < 0;
    public static bool operator >(Tempo left, Tempo right) => left.CompareTo(right) > 0;
    public static bool operator <=(Tempo left, Tempo right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Tempo left, Tempo right) => left.CompareTo(right) >= 0;
}
