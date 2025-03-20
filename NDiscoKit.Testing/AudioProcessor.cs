using CommunityToolkit.HighPerformance;
using CSnakes.Runtime;
using CSnakes.Runtime.Python;
using NAudio.Wave;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NDiscoKit.Testing;
internal sealed class AudioProcessor : IDisposable
{
    private const int frameSize = 2048; // Python: madmom.audio.signal.FRAME_SIZE

    public int Fps { get; }
    public int HopSize { get; }

    public WaveFormat InputFormat { get; }
    public WaveFormat OutputFormat { get; }
    public IBeatTracking BeatTracking { get; }

    private bool disposed = false;

    private readonly ArrayPool<float> floatPool;

    private readonly PyObject tracker;
    private readonly Action<IPyBuffer> writeData;

    private int hopIndex = 0;
    private int hopOffset = 0;
    private readonly byte[] buffer;

    private string? writeHopsTo;
    private WaveFileWriter? debugWriter;

    private AudioProcessor(int fps, WaveFormat inputFormat, IBeatTracking beatTracking, PyObject tracker, Action<IPyBuffer> writeData)
    {
        if (!inputFormat.Equals(new WaveFormat()))
            throw new ArgumentException("PCM 44.1 Khz stereo 16 bit format required.");

        Fps = fps;
        HopSize = ComputeHopSize(fps, inputFormat.SampleRate);

        InputFormat = inputFormat;
        OutputFormat = WaveFormat.CreateIeeeFloatWaveFormat(inputFormat.SampleRate, channels: 1);
        BeatTracking = beatTracking;

        this.tracker = tracker;
        this.writeData = writeData;

        floatPool = ArrayPool<float>.Shared;
        buffer = new byte[frameSize * 4]; // 4 bytes per float

        // DEBUG
        // SetupDesktopDebugFile();
    }

    private void SetupDesktopDebugFile(string dirname = "NDiscoKit Debug", string filename = "record.wav")
    {
        string dir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), dirname);
        Directory.CreateDirectory(dir);
        debugWriter = new(Path.Join(dir, "record.wav"), OutputFormat);

        writeHopsTo = dir;
    }

    public static AudioProcessor CreateTempo(int fps, WaveFormat inputFormat, IBeatTracking beatTracking)
    {
        return new AudioProcessor(
            fps: fps,
            inputFormat: inputFormat,
            beatTracking: beatTracking,
            tracker: beatTracking.CreateTempoTracker(fps),
            writeData: WriteDominantTempo
        );
    }
    public static AudioProcessor CreateTcnTempo(int fps, WaveFormat inputFormat, IBeatTracking beatTracking)
    {
        return new AudioProcessor(
            fps: fps,
            inputFormat: inputFormat,
            beatTracking: beatTracking,
            tracker: beatTracking.CreateTcnTempoTracker(fps),
            writeData: WriteDominantTempo
        );
    }
    public static AudioProcessor CreateBeats(int fps, WaveFormat inputFormat, IBeatTracking beatTracking)
    {
        return new AudioProcessor(
            fps: fps,
            inputFormat: inputFormat,
            beatTracking: beatTracking,
            tracker: beatTracking.CreateBeatTracker(fps),
            writeData: WriteTimings
        );
    }
    public static AudioProcessor CreateOnsets(int fps, WaveFormat inputFormat, IBeatTracking beatTracking)
    {
        return new AudioProcessor(
            fps: fps,
            inputFormat: inputFormat,
            beatTracking: beatTracking,
            tracker: beatTracking.CreateOnsetTracker(fps),
            writeData: WriteTimings
        );
    }

    public void Process(ReadOnlyMemory<byte> bytes) => Process(bytes.Span);

    public void Process(scoped ReadOnlySpan<byte> bytes)
    {
        ThrowIfDisposed();

        ReadOnlySpan<short> stereo16 = MemoryMarshal.Cast<byte, short>(bytes);
        int stereoLength = stereo16.Length;
        int monoLength = stereoLength / 2;

        float[]? mono32Rent = null;
        Span<float> mono32 = monoLength < 512 ? stackalloc float[monoLength] : (mono32Rent = floatPool.Rent(monoLength)).AsSpan(0, monoLength);
        try
        {
            Stereo16ToMono32(stereo16, ref mono32);
            ProcessMono(mono32);
            debugWriter?.Write(MemoryMarshal.Cast<float, byte>(mono32));
        }
        finally
        {
            if (mono32Rent is not null)
            {
                mono32.Clear();
                floatPool.Return(mono32Rent, clearArray: false);
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

    private void ProcessMono(scoped ReadOnlySpan<float> mono32)
    {
        Span<float> buffer = MemoryMarshal.Cast<byte, float>(this.buffer);

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
                hopIndex++;

                ProcessHop(hopIndex);
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
    }

    private static void WriteBuffer(ref Span<float> buffer, scoped ReadOnlySpan<float> data)
    {
        buffer[data.Length..].CopyTo(buffer);
        data.CopyTo(buffer[^data.Length..]);
    }

    private void ProcessHop(int hopIndex)
    {
        if (writeHopsTo is not null && (hopIndex % 100) == 0)
        {
            using WaveFileWriter writer = new(Path.Join(writeHopsTo, $"hop_{hopIndex}.wav"), OutputFormat);
            writer.Write(buffer);
        }

        // Buffer data seems to be intact here after all of the conversions and buffer writes...
        // I verified this by manually listening to the extracted buffer data from hops.
        IPyBuffer result = BeatTracking.ProcessTracker(
            fps: Fps,
            hopIndex: hopIndex,
            hopSize: HopSize,
            frameSize: frameSize,
            buffer: buffer,
            sampleRate: OutputFormat.SampleRate,
            bitsPerSample: OutputFormat.BitsPerSample,
            numChannels: OutputFormat.Channels,
            tracker: tracker
        );
        writeData(result);
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
        debugWriter?.Dispose();
    }

    public static int ComputeHopSize(int fps, int sampleRate)
    {
        int hopSize = sampleRate / fps;

        // Check if value was rounded down during division
        if ((hopSize * fps) != sampleRate)
            throw new ArgumentException("FPS does not round to a specific integer hop size.");

        return hopSize;
    }

    private static void WriteTimings(IPyBuffer dataBuffer)
    {
        foreach (double pos in dataBuffer.AsDoubleReadOnlySpan())
            Console.WriteLine(pos);
    }

    private static void WriteTempoWeights(IPyBuffer dataBuffer)
    {
        Console.Clear();

        ReadOnlySpan2D<double> data = dataBuffer.AsDoubleReadOnlySpan2D();
        int width = data.Width;
        int height = data.Height;
        Debug.Assert(width == 2);
        if (height > 128)
            throw new ArgumentException("Buffer height too large, stack overflow possible.");

        // height is verified before stackalloc
        Span<double> bpms = stackalloc double[height];
        Span<double> strengths = stackalloc double[height];

        for (int i = 0; i < height; i++)
        {
            ReadOnlySpan<double> row = data.GetRowSpan(i);
            Debug.Assert(row.Length == 2);

            bpms[i] = row[0];
            strengths[i] = row[1];
        }

        // Sort by strength
        strengths.Sort(bpms);

        Span<double> top5Bpm = bpms.Length > 5 ? bpms[^5..] : bpms;
        Span<double> top5Strength = strengths.Length > 5 ? strengths[^5..] : strengths;

        // Sort by bpm
        // top5Bpm.Sort(top5Strength);

        for (int i = top5Bpm.Length - 1; i >= 0; i--)
            Console.WriteLine("{0:.00}\t\t{1:P1}", top5Bpm[i], top5Strength[i]);
    }

    private static void WriteSingleDominantTempo(IPyBuffer dataBuffer)
    {
        ReadOnlySpan2D<double> data = dataBuffer.AsDoubleReadOnlySpan2D();

        double max = -1;
        double maxStrength = -1;
        for (int i = 0; i < data.Height; i++)
        {
            ReadOnlySpan<double> row = data.GetRowSpan(i);

            double bpm = row[0];
            double strength = row[1];

            if (maxStrength < strength)
            {
                max = bpm;
                maxStrength = strength;
            }
        }

        Console.WriteLine("{0:.00}", max);
    }

    private static void WriteDominantTempo(IPyBuffer dataBuffer)
    {
        ReadOnlySpan2D<double> data = dataBuffer.AsDoubleReadOnlySpan2D();
        int width = data.Width;
        int height = data.Height;
        Debug.Assert(width == 2);
        if (height > 128)
            throw new ArgumentException($"Buffer height too large, stack overflow possible: {height}");

        // height is verified before stackalloc
        Span<double> bpms = stackalloc double[height];
        Span<double> strengths = stackalloc double[height];

        for (int i = 0; i < height; i++)
        {
            ReadOnlySpan<double> row = data.GetRowSpan(i);
            Debug.Assert(row.Length == 2);

            bpms[i] = row[0];
            strengths[i] = row[1];
        }

        strengths.Sort(bpms);

        if (bpms.Length > 1)
        {
            double t1 = bpms[^1];
            double t2 = bpms[^2];
            if (t2 > t1)
                Console.WriteLine("{0:.00}   {1:.00}", t1, t2);
            else
                Console.WriteLine("{1:.00}   {0:.00}", t1, t2);
        }
    }
}
