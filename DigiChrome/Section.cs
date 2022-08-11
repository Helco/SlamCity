namespace DigiChrome;
using System;
using System.IO;
using static DigiChrome.Utils;

internal enum SectionType : byte
{
    Audio = 0xA2,
    Color = 0x81,
    Combined = 0xF1
}

internal struct Section
{
    public readonly SectionType Type;
    public readonly byte Track;
    public readonly ushort Size;

    public Section(ref ReadOnlySpan<byte> data)
    {
        Type = (SectionType)ReadU8(ref data);
        Track = ReadU8(ref data);
        Size = ReadU16(ref data);
    }

    public static Section? TryRead(Stream stream)
    {
        Span<byte> totalData = stackalloc byte[4];
        if (stream.Read(totalData) != totalData.Length)
            return null;
        ReadOnlySpan<byte> data = totalData;
        return new Section(ref data);
    }
}
