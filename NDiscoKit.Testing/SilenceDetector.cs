using NAudio.Wave;
using System.Runtime.InteropServices;

namespace NDiscoKit.Testing;
internal class SilenceDetector
{
    private readonly short[] _buffer;
    private int _bufferSize = 0;

    public SilenceDetector(TimeSpan duration, WaveFormat format)
    {
        if (!format.Equals(new WaveFormat()))
            throw new ArgumentException("PCM 44.1 kHz stereo 16 bit format required.");

        _buffer = new short[GetBufferSize(duration, format.SampleRate)];
    }

    private bool BufferIsFull => _bufferSize >= _buffer.Length;
    public bool IsSilence { get; private set; }

    private static int GetBufferSize(TimeSpan duration, int sampleRate)
    {
        double size = duration.TotalSeconds * sampleRate;
        return checked((int)size);
    }

    public void Reset()
    {
        _bufferSize = 0;
        IsSilence = false;
    }

    public void Update(scoped ReadOnlySpan<byte> data)
    {
        WriteBuffer(MemoryMarshal.Cast<byte, short>(data));
        IsSilence = BufferIsFull && GetIsSilence(_buffer);
    }

    private void WriteBuffer(scoped ReadOnlySpan<short> data)
    {
        Span<short> buffer = _buffer.AsSpan();
        buffer[data.Length..].CopyTo(buffer);
        data.CopyTo(buffer[^data.Length..]);

        if ((_bufferSize + data.Length) < _buffer.Length)
            _bufferSize += data.Length;
        else
            _bufferSize = _buffer.Length;
    }

    // https://github.com/naudio/NAudio/issues/320#issuecomment-380832081
    private static bool GetIsSilence(Span<short> buffer)
    {
        // We don't care how many channels we have, this method will check all provided samples

        const short SILENCE_MAX = 130;
        const short SILENCE_MIN = -130;

        foreach (short sample in buffer)
        {
            if (sample > SILENCE_MAX)
                return false;
            if (sample < SILENCE_MIN)
                return false;
        }

        return true;
    }
}
