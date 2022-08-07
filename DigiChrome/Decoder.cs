namespace DigiChrome;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = sizeof(ushort))]
public struct Color
{
    private readonly ushort raw;

    public Color(ushort raw) => this.raw = raw;

    public byte R => (byte)((raw >> 10) & 0b11111);
    public byte G => (byte)((raw >> 5) & 0b11111);
    public byte B => (byte)((raw >> 0) & 0b11111);
}

public class Frame
{
    private readonly Decoder parent;

    internal Frame(Decoder parent) => this.parent = parent;

    public int Width => parent.colorDecoder.Width;
    public int Height => parent.colorDecoder.Height;
    public ReadOnlySpan<Color> Palette => parent.colorDecoder.Palette;
    public ReadOnlySpan<byte> Color => parent.colorDecoder.Target;
}

internal enum SectionType : byte
{
    Audio = 0xA2,
    Color = 0x81,
    Combined = 0xF1
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct Section
{
    public SectionType type;
    public byte track;
    public ushort size;
}

public unsafe class Decoder : IDisposable, IEnumerator<Frame>
{
    internal readonly ColorDecoder colorDecoder = new ColorDecoder();
    private readonly Frame frame;
    private readonly Stream source;
    private readonly bool shouldDisposeStream;
    private byte[] frameBuffer;
    private MemoryStream frameStream;
    private BinaryReader frameReader;
    private bool disposedValue;

    public Frame Current => frame;
    object IEnumerator.Current => Current;

    public Decoder(Stream source, bool shouldDisposeStream = true)
    {
        this.source = source;
        this.shouldDisposeStream = shouldDisposeStream;
        frame = new Frame(this);

        frameBuffer = null!;
        frameStream = null!;
        frameReader = null!;
        EnsureFrameBufferSize(21000); // max frame size in "Slam City with Scottie Pippen"
    }

    public void Reset()
    {
        source.Position = 0;
    }

    public bool MoveNext()
    {
        Section section;
        int sectionRead = source.Read(new Span<byte>(&section, sizeof(Section)));
        if (sectionRead == 0)
            return false;
        if (sectionRead != sizeof(Section))
            throw new EndOfStreamException("Could not read section header");
        if (section.type != SectionType.Combined)
            throw new NotSupportedException("Not supporting non-combined sections right now");

        EnsureFrameBufferSize(section.size);
        if (source.Read(frameBuffer, 0, section.size) != section.size)
            throw new EndOfStreamException("Could not read combined section content");
        fixed (void* frameBufferPtr = frameBuffer)
            section = *(Section*)frameBufferPtr;
        if (section.type != SectionType.Color)
            throw new NotSupportedException("Unsupported packet at start of combined section");
        var colorSection = frameBuffer.AsSpan(0, section.size + sizeof(Section));
        colorDecoder.Decode(colorSection);
        return true;
    }

    private void EnsureFrameBufferSize(int size)
    {
        if (frameBuffer?.Length >= size)
            return;
        frameReader?.Dispose();
        frameBuffer = new byte[size];
        frameStream = new MemoryStream(frameBuffer, writable: true);
        frameReader = new BinaryReader(frameStream);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue)
            return;
        disposedValue = true;
        if (!disposing)
            return;
        if (shouldDisposeStream)
            source.Dispose();
        frameReader.Dispose();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
