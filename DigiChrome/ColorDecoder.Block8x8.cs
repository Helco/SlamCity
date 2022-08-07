namespace DigiChrome;
using System;
using static DigiChrome.Utils;

unsafe partial class ColorDecoder
{
    private void Block8x8(int x, int y, ref ReadOnlySpan<byte> data, byte* outPtr)
    {
        int fixedPatternI = Indices[data[0]];
        if (fixedPatternI > 0)
            Block8x8_FixedPattern(ref data, outPtr);
        else if (data[0] == TypeCopyFull)
            Block_Copy(x, y, BlockSize, BlockSize, outPtr);
        else
            Block8x8_EmbeddedPattern(ref data, outPtr);
    }

    private void Block8x8_FixedPattern(ref ReadOnlySpan<byte> data, byte* outPtr)
    {
        var pattern = FixedPatterns[Indices[data[0]] - 1];
        ReadOnlySpan<byte> colors = GetColors(ref data);
        Block8x8_Pattern(colors, pattern, outPtr);
    }

    private void Block8x8_EmbeddedPattern(ref ReadOnlySpan<byte> data, byte* outPtr)
    {
        var colors = GetColors(ref data);
        var pattern = SwapToBE(BitConverter.ToUInt64(data));
        data = data[sizeof(ulong)..];
        Block8x8_Pattern(colors, pattern, outPtr);
    }

    private void Block8x8_Pattern(ReadOnlySpan<byte> colors, ulong pattern, byte* outPtr)
    {
        for (int i = 0; i < BlockSize; i++)
        {
            outPtr[7] = colors[PopHighestBit(ref pattern)];
            outPtr[6] = colors[PopHighestBit(ref pattern)];
            outPtr[5] = colors[PopHighestBit(ref pattern)];
            outPtr[4] = colors[PopHighestBit(ref pattern)];
            outPtr[3] = colors[PopHighestBit(ref pattern)];
            outPtr[2] = colors[PopHighestBit(ref pattern)];
            outPtr[1] = colors[PopHighestBit(ref pattern)];
            outPtr[0] = colors[PopHighestBit(ref pattern)];
            outPtr += curWidth * BlockSize;
        }
    }
}
