using CommunityToolkit.HighPerformance;
using CSnakes.Runtime.Python;
using NAudio.Wave;
using NDiscoKit.AudioAnalysis.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NDiscoKit.AudioAnalysis.Processors;

/// <summary>
/// Not thread-safe.
/// </summary>
internal sealed class MadmomProcessor : IDisposable
{
    public readonly struct Configuration
    {
        public Configuration(WaveFormat format, int fps = 100)
        {
            FPS = fps;
            Format = format;

            FrameSize = 2048; // Python: madmom.audio.signal.FRAME_SIZE
            HopSize = ComputeHopSize(fps, format.SampleRate);
        }

        public int FPS { get; }
        public WaveFormat Format { get; }

        public int FrameSize { get; }
        public int HopSize { get; }

        private static int ComputeHopSize(int fps, int sampleRate)
        {
            int hopSize = sampleRate / fps;

            // Check if value was rounded down during division
            if (hopSize * fps != sampleRate)
                throw new ArgumentException("FPS does not round to a specific integer hop size.");

            return hopSize;
        }
    }



    public Configuration Config { get; }
    public IBeatTracking BeatTracking { get; }

    private bool disposed = false;

    private readonly PyObject tracker;

    private double currentTime = 0d;
    private int hopIndex = 0;
    private int hopOffset = 0;
    private readonly byte[] buffer;

    private MadmomProcessor(Configuration config, IBeatTracking beatTracking, PyObject tracker)
    {
        Config = config;

        BeatTracking = beatTracking;

        this.tracker = tracker;

        buffer = new byte[config.FrameSize * 4]; // 4 bytes per float
    }

    public static MadmomProcessor Create(Configuration config, IBeatTracking beatTracking)
    {
        return new MadmomProcessor(
            config: config,
            beatTracking: beatTracking,
            tracker: beatTracking.CreateProcessor(config.FPS)
        );
    }

    public void Process(scoped in ReadOnlySpan<float> data, in AudioProcessorResult? result, bool reset = false)
    {
        ThrowIfDisposed();

        Span<float> buffer = MemoryMarshal.Cast<byte, float>(this.buffer);

        result?.BeforeProcess();

        int hopSize = Config.HopSize;
        int sampleRate = Config.Format.SampleRate;

        int index = 0;
        while (index < data.Length)
        {
            int remaining = data.Length - index;
            int writeCount = hopSize - hopOffset;
            if (remaining > writeCount)
            {
                // We have enough data for a full hop

                WriteBuffer(ref buffer, data.Slice(index, writeCount));
                index += writeCount;

                Debug.Assert((hopOffset + writeCount) % hopSize == 0);
                hopOffset = 0;

                if (reset)
                    hopIndex = 0;
                ProcessHop(in result, hopIndex, reset: reset);
                hopIndex++;
                reset = false;
            }
            else
            {
                ReadOnlySpan<float> remainingData = data[index..];
                Debug.Assert(remainingData.Length == remaining);

                WriteBuffer(ref buffer, in remainingData);
                index += remainingData.Length;
                hopOffset += remainingData.Length;
            }
        }

        currentTime = ((hopIndex * hopSize) + hopOffset) / sampleRate;
    }

    private static void WriteBuffer(scoped ref readonly Span<float> buffer, scoped in ReadOnlySpan<float> data)
    {
        buffer[data.Length..].CopyTo(buffer);
        data.CopyTo(buffer[^data.Length..]);
    }

    private void ProcessHop(ref readonly AudioProcessorResult? apResult, int hopIndex, bool reset)
    {
        // Buffer data seems to be intact here after all of the conversions and buffer writes...
        // I verified this by manually listening to the extracted buffer data from hops.
        IReadOnlyList<IPyBuffer> result = BeatTracking.ProcessTrackers(
            fps: Config.FPS,
            hopIndex: hopIndex,
            hopSize: Config.HopSize,
            frameSize: Config.FrameSize,
            buffer: buffer,
            tracker: tracker,
            reset: reset,
            sampleRate: Config.Format.SampleRate,
            bitsPerSample: 8 * sizeof(float),
            numChannels: Config.Format.Channels
        );

        IPyBuffer beats = result[0];
        IPyBuffer tempo = result[1];

        (Prediction<Tempo>? T1, Prediction<Tempo>? T2) = GetDominantTempo(tempo);
        apResult?.AfterHop(
            t1: T1,
            t2: T2,
            beats: beats.AsReadOnlySpan<double>(),
            reset: reset
        );
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
            throw new InvalidOperationException("Object disposed.");
    }

    public void Dispose()
    {
        disposed = true;
        tracker?.Dispose();
    }

    private static Prediction<Tempo>? ConstructTempoPrediction(in ReadOnlySpan<double> span)
    {
        if (span.Length != 2)
            throw new ArgumentException("Invalid span length.");

        double tempo = span[0];
        if (double.IsNaN(tempo))
            return null;

        return new Prediction<Tempo>(Strength: span[1], Value: new Tempo(tempo));
    }

    private static (Prediction<Tempo>? T1, Prediction<Tempo>? T2) GetDominantTempo(IPyBuffer dataBuffer)
    {
        ReadOnlySpan2D<double> data = dataBuffer.AsDoubleReadOnlySpan2D();

#if DEBUG
        for (int i = 1; i < data.Height; i++)
        {
            ReadOnlySpan<double> r1 = data.GetRowSpan(i - 1);
            ReadOnlySpan<double> r2 = data.GetRowSpan(i);
            if (!(r1.Length == 2 && r2.Length == 2 && r1[1] > r2[1]))
                Debug.Fail("Strengths should be sorted from largest to smallest.");
        }
#endif

        Prediction<Tempo>? t1 = data.Height > 0 ? ConstructTempoPrediction(data.GetRowSpan(0)) : null;
        Prediction<Tempo>? t2 = data.Height > 1 ? ConstructTempoPrediction(data.GetRowSpan(1)) : null;

        // Non-null value first if possible or both are null
        if (!t1.HasValue)
            return (t2, t1);
        if (!t2.HasValue)
            return (t1, t2);

        // Smaller tempo first
        if (t2.Value.Value > t1.Value.Value)
            return (t1, t2);
        else
            return (t2, t1);
    }
}