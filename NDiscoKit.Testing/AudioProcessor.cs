using CommunityToolkit.HighPerformance;
using CSnakes.Runtime;
using CSnakes.Runtime.Python;
using NAudio.Wave;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NDiscoKit.Testing;

/// <summary>
/// Not thread-safe.
/// </summary>
internal sealed class AudioProcessor : IDisposable
{
    /// <summary>
    /// The size of <see cref="GetBuffer"/>
    /// </summary>
    public const int FrameSize = 2048; // Python: madmom.audio.signal.FRAME_SIZE

    public int Fps { get; }
    public int HopSize { get; }

    public WaveFormat InputFormat { get; }
    public WaveFormat OutputFormat { get; }
    public IBeatTracking BeatTracking { get; }

    private bool disposed = false;

    private readonly PyObject tracker;

    private double currentTime = 0d;
    private int hopIndex = 0;
    private int hopOffset = 0;
    private readonly byte[] buffer;

    private AudioProcessor(int fps, WaveFormat inputFormat, IBeatTracking beatTracking, PyObject tracker)
    {
        if (!inputFormat.Equals(new WaveFormat()))
            throw new ArgumentException("PCM 44.1 kHz stereo 16 bit format required.");

        Fps = fps;
        HopSize = ComputeHopSize(fps, inputFormat.SampleRate);

        InputFormat = inputFormat;
        OutputFormat = WaveFormat.CreateIeeeFloatWaveFormat(inputFormat.SampleRate, channels: 1);
        BeatTracking = beatTracking;

        this.tracker = tracker;

        buffer = new byte[FrameSize * 4]; // 4 bytes per float
    }

    public ReadOnlySpan<float> GetBuffer() => MemoryMarshal.Cast<byte, float>(buffer);

    public static AudioProcessor Create(int fps, WaveFormat inputFormat, IBeatTracking beatTracking)
    {
        return new AudioProcessor(
            fps: fps,
            inputFormat: inputFormat,
            beatTracking: beatTracking,
            tracker: beatTracking.CreateProcessor(fps)
        );
    }

    public void Process(scoped ReadOnlySpan<byte> bytes, in AudioProcessorResult? result = null, bool reset = false)
    {
        ThrowIfDisposed();

        ReadOnlySpan<short> stereo16 = MemoryMarshal.Cast<byte, short>(bytes);
        int stereoLength = stereo16.Length;
        int monoLength = stereoLength / 2;

        float[]? mono32Rent = null;
        Span<float> mono32 = monoLength < 512 ? stackalloc float[monoLength] : (mono32Rent = ArrayPool<float>.Shared.Rent(monoLength)).AsSpan(0, monoLength);
        try
        {
            Stereo16ToMono32(stereo16, ref mono32);
            ProcessMono(mono32, in result, reset: reset);
        }
        finally
        {
            if (mono32Rent is not null)
            {
                mono32.Clear();
                ArrayPool<float>.Shared.Return(mono32Rent, clearArray: false);
            }
        }
    }

    private static void Stereo16ToMono32(scoped ReadOnlySpan<short> stereo16, ref Span<float> mono32)
    {
        if (stereo16.Length != 2 * mono32.Length)
            throw new ArgumentException("stereo16 must be exactly twice as large as mono32");

        for (int i = 0; i < mono32.Length; i++)
        {
            short left = stereo16[i * 2];
            short right = stereo16[(i * 2) + 1];

            // Take the average of the two channels and convert it to float
            // Equals to: ((left + right) / 2) / short.MaxValue
            mono32[i] = (left + right) / (float)(short.MaxValue * 2);
        }
    }

    private void ProcessMono(scoped ReadOnlySpan<float> mono32, ref readonly AudioProcessorResult? result, bool reset)
    {
        Span<float> buffer = MemoryMarshal.Cast<byte, float>(this.buffer);

        result?.BeforeProcess();

        int index = 0;
        while (index < mono32.Length)
        {
            int remaining = mono32.Length - index;
            int writeCount = HopSize - hopOffset;
            if (remaining > writeCount)
            {
                // We have enough data for a full hop

                WriteBuffer(ref buffer, mono32.Slice(index, writeCount));
                index += writeCount;

                Debug.Assert((hopOffset + writeCount) % HopSize == 0);
                hopOffset = 0;

                if (reset)
                    hopIndex = 0;
                ProcessHop(in result, hopIndex, reset: reset);
                hopIndex++;
                reset = false;
            }
            else
            {
                ReadOnlySpan<float> remainingData = mono32[index..];
                Debug.Assert(remainingData.Length == remaining);

                WriteBuffer(ref buffer, remainingData);
                index += remainingData.Length;
                hopOffset += remainingData.Length;
            }
        }

        currentTime = ((hopIndex * HopSize) + hopOffset) / OutputFormat.SampleRate;
    }

    private static void WriteBuffer(scoped ref Span<float> buffer, scoped ReadOnlySpan<float> data)
    {
        buffer[data.Length..].CopyTo(buffer);
        data.CopyTo(buffer[^data.Length..]);
    }

    private void ProcessHop(ref readonly AudioProcessorResult? apResult, int hopIndex, bool reset)
    {
        // Buffer data seems to be intact here after all of the conversions and buffer writes...
        // I verified this by manually listening to the extracted buffer data from hops.
        IReadOnlyList<IPyBuffer> result = BeatTracking.ProcessTrackers(
            fps: Fps,
            hopIndex: hopIndex,
            hopSize: HopSize,
            frameSize: FrameSize,
            buffer: buffer,
            tracker: tracker,
            reset: reset,
            sampleRate: OutputFormat.SampleRate,
            bitsPerSample: OutputFormat.BitsPerSample,
            numChannels: OutputFormat.Channels
        );

        IPyBuffer beats = result[0];
        IPyBuffer tempo = result[1];

        (Prediction<Tempo>? T1, Prediction<Tempo>? T2) = GetDominantTempo(tempo);
        apResult?.AfterHop(
            t1: T1,
            t2: T2,
            beats: beats.AsReadOnlySpan<double>()
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

    public static int ComputeHopSize(int fps, int sampleRate)
    {
        int hopSize = sampleRate / fps;

        // Check if value was rounded down during division
        if ((hopSize * fps) != sampleRate)
            throw new ArgumentException("FPS does not round to a specific integer hop size.");

        return hopSize;
    }

    private static (Prediction<Tempo>? T1, Prediction<Tempo>? T2) GetDominantTempo(IPyBuffer dataBuffer)
    {
        static Prediction<Tempo>? TryConstructPrediction(in ReadOnlySpan<double> span)
        {
            if (span.Length != 2)
                throw new ArgumentException("Invalid span length.");

            double tempo = span[0];
            if (double.IsNaN(tempo))
                return null;

            return new Prediction<Tempo>(Strength: span[1], Value: new Tempo(tempo));
        }

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

        Prediction<Tempo>? t1 = data.Height > 0 ? TryConstructPrediction(data.GetRowSpan(0)) : null;
        Prediction<Tempo>? t2 = data.Height > 1 ? TryConstructPrediction(data.GetRowSpan(1)) : null;

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
