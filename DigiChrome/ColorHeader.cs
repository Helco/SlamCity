namespace DigiChrome;
using System;
using static DigiChrome.Utils;

internal struct ColorHeader
{
    public readonly uint Unknown1;
    public readonly byte Unknown2;
    public readonly byte ColorCount;
    public readonly byte Width;
    public readonly byte Height;

    public ColorHeader(ref ReadOnlySpan<byte> data)
    {
        Unknown1 = ReadU32(ref data);
        Unknown2 = ReadU8(ref data);
        ColorCount = ReadU8(ref data);
        Width = ReadU8(ref data);
        Height = ReadU8(ref data);
    }
}
