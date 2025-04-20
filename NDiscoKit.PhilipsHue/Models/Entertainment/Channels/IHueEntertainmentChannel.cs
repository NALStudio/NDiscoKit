using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NDiscoKit.PhilipsHue.Models.Entertainment.Channels;
public interface IHueEntertainmentChannel
{
    const int BytesPerChannel = 7;

    abstract byte Id { get; }

    protected abstract ushort ColorChannel1 { get; }
    protected abstract ushort ColorChannel2 { get; }
    protected abstract ushort ColorChannel3 { get; }

    sealed void SetBytes(scoped Span<byte> bytes)
    {
        if (bytes.Length != BytesPerChannel)
            throw new ArgumentException($"Expected {BytesPerChannel} bytes, got {bytes.Length} instead.");

        bytes[0] = Id;
        SetColor(bytes[1..3], ColorChannel1);
        SetColor(bytes[3..5], ColorChannel2);
        SetColor(bytes[5..7], ColorChannel3);
    }

    private static void SetColor(scoped Span<byte> bytes, ushort color)
    {
        if (bytes.Length != sizeof(ushort))
            throw new ArgumentException("Invalid input length.", nameof(bytes));

        // Stolen from BitConverter.TryWriteBytes
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(bytes), color);
        if (BitConverter.IsLittleEndian)
            bytes.Reverse();
    }
}
