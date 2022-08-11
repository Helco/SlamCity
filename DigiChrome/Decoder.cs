namespace DigiChrome;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public unsafe class Decoder : IDisposable, IEnumerator<Frame>
{
    private static readonly int SectionSize = 4;
    private static readonly int AudioHeaderSize = 9; // no useful information in the audio headers

    internal readonly ColorDecoder colorDecoder = new ColorDecoder();
    private readonly Frame frame;
    private readonly Stream source;
    private readonly bool shouldDisposeStream;
    private readonly bool strictValidation;

    private Section? nextSection;
    private byte[] frameBuffer;
    private MemoryStream frameStream;
    private bool disposedValue;
    private Range audioDataRange;

    public Frame Current => frame;
    object IEnumerator.Current => Current;
    internal ReadOnlySpan<byte> CurrentAudio => MemoryMarshal.Cast<byte, byte>(frameBuffer[audioDataRange]);

    public Decoder(Stream source, bool shouldDisposeStream = true, bool strictValidation = true)
    {
        this.source = source;
        this.shouldDisposeStream = shouldDisposeStream;
        this.strictValidation = strictValidation;
        frame = new Frame(this);

        frameBuffer = null!;
        frameStream = null!;
        EnsureFrameBufferSize(21000); // max frame size in "Slam City with Scottie Pippen"
    }

    public void Reset()
    {
        source.Position = 0;
    }

    public bool MoveNext()
    {
        audioDataRange = default;
        colorDecoder.Clear();

        var section = nextSection ?? Section.TryRead(source);
        if (!section.HasValue)
            return false;
        nextSection = null;
        

        EnsureFrameBufferSize(section.Value.Size);
        if (source.Read(frameBuffer, 0, section.Value.Size) != section.Value.Size)
            throw new EndOfStreamException("Could not read section content");

        switch (section.Value.Type)
        {
            case SectionType.Combined:
                CombinedSection(section.Value);
                return true;

            case SectionType.Color:
                ColorSection(section.Value);
                ReadPotentialPair(SectionType.Audio, AudioSection);
                return true;

            case SectionType.Audio:
                AudioSection(section.Value);
                ReadPotentialPair(SectionType.Color, ColorSection);
                return true;

            case var _ when strictValidation:
                throw new InvalidDataException($"Unknown section type {section.Value.Type.ToString("X2")}");
        }
        return true;
    }

    private void CombinedSection(Section combinedSection)
    {
        ReadOnlySpan<byte> data = frameBuffer.AsSpan();

        var colorSection = new Section(ref data);
        if (colorSection.Type != SectionType.Color)
            throw new InvalidDataException("Expected color section in combined section");
        colorDecoder.Decode(data[0..colorSection.Size]);
        data = data[colorSection.Size..];

        var audioDataStart = SectionSize * 2 + colorSection.Size;
        if (combinedSection.Size < audioDataStart)
            return;

        var audioSection = new Section(ref data);
        if (audioSection.Type != SectionType.Audio)
            throw new InvalidDataException("Expected audio section in combined section");
        AudioSection(audioSection, audioDataStart);
    }

    private void ReadPotentialPair(SectionType expectedNextType, Action<Section, int> readAction)
    {
        var section = Section.TryRead(source);
        if (section?.Type == expectedNextType)
        {
            EnsureFrameBufferSize(section.Value.Size);
            if (source.Read(frameBuffer, 0, section.Value.Size) != section.Value.Size)
                throw new EndOfStreamException("Could not read section content");
            readAction(section.Value, 0);
        }
        else
            nextSection = section;
    }

    private void ColorSection(Section section, int offset = 0)
    {
        if (frameBuffer.Length < offset + section.Size)
            throw new InvalidDataException("Section is not large enough for reported color section");
        colorDecoder.Decode(frameBuffer[offset..(offset + section.Size)]);
    }

    private void AudioSection(Section section, int offset = 0)
    {
        if (frameBuffer.Length < offset + section.Size)
            throw new InvalidDataException("Section is not large enough for reported audio section");
        if (section.Size < AudioHeaderSize)
            throw new InvalidDataException("Audio section is too small");
        audioDataRange = (offset + AudioHeaderSize)..(offset + section.Size);
    }

    private void EnsureFrameBufferSize(int size)
    {
        if (frameBuffer?.Length >= size)
            return;
        frameBuffer = new byte[size];
        frameStream = new MemoryStream(frameBuffer, writable: true);
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
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
