namespace NDiscoKit.AudioAnalysis.Processors;

public class SilenceDetector
{
    private readonly float[] _buffer;
    private int _approxBufferSize;

    public SilenceDetector(int bufferSize)
    {
        _buffer = new float[bufferSize];
    }

    public SilenceDetector(TimeSpan duration, int sampleRate) : this(GetBufferSize(duration, sampleRate))
    {
    }

    private bool BufferIsFull => _approxBufferSize >= _buffer.Length;
    public bool IsSilence { get; private set; }

    public static int GetBufferSize(TimeSpan duration, int sampleRate)
    {
        double size = duration.TotalSeconds * sampleRate;
        return checked((int)size);
    }

    public void Reset()
    {
        _approxBufferSize = 0;
        IsSilence = false;
    }

    public void Process(in ReadOnlySpan<float> data)
    {
        WriteBuffer(in data);
        IsSilence = BufferIsFull && GetIsSilence(_buffer);
    }

    private void WriteBuffer(in ReadOnlySpan<float> data)
    {
        Span<float> buffer = _buffer.AsSpan();
        buffer[data.Length..].CopyTo(buffer);
        data.CopyTo(buffer[^data.Length..]);

        if (_approxBufferSize < _buffer.Length)
            _approxBufferSize += data.Length;
    }

    // https://github.com/naudio/NAudio/issues/320#issuecomment-380832081
    private static bool GetIsSilence(ReadOnlySpan<float> buffer)
    {
        // We don't care how many channels we have, this method will check all provided samples

        const float SILENCE_MAX = 130 / (float)short.MaxValue;
        const float SILENCE_MIN = -130 / (float)short.MaxValue;

        foreach (float sample in buffer)
        {
            if (sample > SILENCE_MAX)
                return false;
            if (sample < SILENCE_MIN)
                return false;
        }

        return true;
    }
}