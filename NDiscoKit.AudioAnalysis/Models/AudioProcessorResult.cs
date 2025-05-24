using System.Collections.ObjectModel;

namespace NDiscoKit.AudioAnalysis.Models;

public class AudioProcessorResult
{
    public Prediction<Tempo>? T1 { get; private set; }
    public Prediction<Tempo>? T2 { get; private set; }

    public List<double> Beats { get; } = new(capacity: 512);
    public bool WasReset { get; private set; }


    public void BeforeProcess()
    {
        WasReset = false;
    }

    public void AfterHop(Prediction<Tempo>? t1, Prediction<Tempo>? t2, ReadOnlySpan<double> beats, bool reset)
    {
        T1 = t1;
        T2 = t2;

        if (reset)
        {
            Beats.Clear();
            WasReset = true;
        }

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

public class ReadOnlyAudioProcessorResult
{
    private readonly AudioProcessorResult _result;
    public ReadOnlyAudioProcessorResult(AudioProcessorResult result)
    {
        _result = result;

        Beats = _result.Beats.AsReadOnly();
    }

    public Prediction<Tempo>? T1 => _result.T1;
    public Prediction<Tempo>? T2 => _result.T2;

    public ReadOnlyCollection<double> Beats { get; }
    public bool WasReset => _result.WasReset;
}