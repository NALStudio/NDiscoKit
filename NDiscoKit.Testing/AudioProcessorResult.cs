namespace NDiscoKit.Testing;
internal class AudioProcessorResult
{
    public Prediction<Tempo>? T1 { get; private set; }
    public Prediction<Tempo>? T2 { get; private set; }

    public List<double> Beats { get; } = new(capacity: 128);

    public void Update(Prediction<Tempo>? t1, Prediction<Tempo>? t2, ReadOnlySpan<double> beats)
    {
        T1 = t1;
        T2 = t2;

        Beats.AddRange(beats);
    }

    public IEnumerable<double> BeatsAfter(double time)
    {
        int i = Beats.Count - 1;
        while (i >= 0 && Beats[i] > time)
            i--;

        if (i < 0)
            yield break;

        for (int j = i; j < Beats.Count; j++)
            yield return Beats[j];
    }
}
