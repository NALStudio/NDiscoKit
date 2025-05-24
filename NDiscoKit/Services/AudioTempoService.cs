using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using NDiscoKit.AudioAnalysis.Models;
using NDiscoKit.AudioAnalysis.Processors;
using NDiscoKit.Python;

namespace NDiscoKit.Services;
internal class AudioTempoService : IDisposable
{
    private record class Processors(MadmomProcessor Madmom, SilenceDetector Silence) : IDisposable
    {
        public void Dispose()
        {
            Madmom.Dispose();
        }
    }

    public readonly record struct TempoCollection(Tempo T1, Tempo T2);
    private const double _kDefaultTempo = 60d;

    private readonly ILogger<AudioTempoService> logger;
    private readonly AudioRecordingService recorder;
    private readonly IPythonService pythonService;
    public AudioTempoService(ILogger<AudioTempoService> logger, AudioRecordingService recorder, IPythonService python)
    {
        this.logger = logger;

        this.recorder = recorder;
        recorder.DataAvailable += Recorder_DataAvailable;

        this.pythonService = python;

        SetTempo(null, null);
    }

    private TempoCollection _tempos;

    /// <summary>
    /// The slower tempo detected.
    /// </summary>
    public Tempo T1
    {
        get => _tempos.T1;
        set => SetTempo(t1: value, t2: null);
    }

    /// <summary>
    /// The faster tempo detected.
    /// </summary>
    public Tempo T2
    {
        get => _tempos.T2;
        set => SetTempo(t1: null, t2: value);
    }

    public event EventHandler<TempoCollection>? TempoChanged;

    private NDKPython? _python;
    private Processors? _processors;

    public bool Auto { get; private set; }

    public async ValueTask SetAutoAsync(bool auto)
    {
        Auto = auto;
        if (auto)
        {
            _python ??= await pythonService.GetPythonAsync();
            LoadProcessor(_python);
        }
        else
        {
            UnloadProcessor();
        }
    }

    private void UnloadProcessor()
    {
        Processors? p = Interlocked.Exchange(ref _processors, null);
        p?.Dispose();
    }

    private void LoadProcessor(NDKPython python)
    {
        MadmomProcessor madmom = MadmomProcessor.Create(
            new MadmomProcessor.Configuration(recorder.OutputFormat),
            python.Python.BeatTracking()
        );
        SilenceDetector silence = new(TimeSpan.FromSeconds(5), recorder.OutputFormat.SampleRate);

        Processors newValue = new(
            Madmom: madmom,
            Silence: silence
        );
        bool replaced = Interlocked.CompareExchange(ref _processors, newValue, null) is null;
        if (!replaced)
            newValue.Dispose();
    }

    private void Recorder_DataAvailable(object? sender, ReadOnlyMemory<float> e)
    {
        Processors? p = _processors;
        if (p is null)
            return;

        ReadOnlySpan<float> data = e.Span;

        p.Silence.Process(in data);

        bool reset = p.Silence.IsSilence;
        p.Madmom.Process(in data, reset: reset);

        ReadOnlyAudioProcessorResult result = p.Madmom.Result;
        if (result.WasReset)
        {
            p.Silence.Reset();
            logger.LogInformation("Processor was reset during silence.");
        }

        SetTempo(result.T1, result.T2, throwIfAuto: false);
    }

    private void SetTempo(Tempo? t1, Tempo? t2, bool throwIfAuto = true)
    {
        if (throwIfAuto && Auto)
            throw new InvalidOperationException("Cannot set tempo: auto tempo is enabled.");

        if (!t1.HasValue)
        {
            if (t2.HasValue)
                t1 = t2.Value / 2d;
            else
                t1 = new Tempo(_kDefaultTempo); // Default tempo if no tempo available
        }

        if (!t2.HasValue)
            t2 = 2d * t1;

        TempoCollection tempos = new(T1: t1.Value, T2: t2.Value);
        if (tempos != _tempos)
        {
            _tempos = tempos;
            TempoChanged?.Invoke(this, tempos);
        }
    }

    public void Dispose()
    {
        recorder.DataAvailable -= Recorder_DataAvailable;
        UnloadProcessor();
    }
}
