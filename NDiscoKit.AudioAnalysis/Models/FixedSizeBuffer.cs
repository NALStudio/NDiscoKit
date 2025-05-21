using System.Diagnostics;

namespace NDiscoKit.AudioAnalysis.Models;
public class FixedSizeBuffer<T>
{
    public int Capacity => _buffer.Length;

    private readonly T[] _buffer;

    /// <summary>
    /// Initialize the buffer with default values of <typeparamref name="T"/>.
    /// </summary>
    public FixedSizeBuffer(int size)
    {
        _buffer = new T[size];
    }

    public Span<T> AsSpan() => _buffer.AsSpan();
    public Span<T> AsSpan(int start) => _buffer.AsSpan(start);
    public Span<T> AsSpan(int start, int length) => _buffer.AsSpan(start, length);

    /// <summary>
    /// Push new data into the buffer pushing all the values left in the process. First (oldest) elements will be removed.
    /// </summary>
    /// <remarks>
    /// If <paramref name="values"/> length is longer than the size of the buffer, only the last N elements are added.
    /// </remarks>
    public void Push(scoped in ReadOnlySpan<T> values)
    {
        if (values.Length >= _buffer.Length)
        {
            ReadOnlySpan<T> v = values[^_buffer.Length..];
            Debug.Assert(v.Length == _buffer.Length);
            v.CopyTo(_buffer);
        }
        else
        {
            _buffer.AsSpan(values.Length).CopyTo(_buffer);
            values.CopyTo(_buffer.AsSpan(_buffer.Length - values.Length));
        }
    }

    public void Clear() => Array.Clear(_buffer);
}
