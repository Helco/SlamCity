﻿namespace DigiChrome;
using System;
using static DigiChrome.Utils;

unsafe partial class ColorDecoder
{
    private void Block8x8(int x, int y, ref ReadOnlySpan<byte> data, byte* outPtr)
    {
        var blockType = data[0];
        int fixedPatternI = Indices[blockType];
        if (fixedPatternI > 0)
            Block8x8_FixedPattern(ref data, outPtr);
        else if (blockType == TypeCopyFull)
        {
            Block_Copy(x, y, BlockSize, BlockSize, outPtr);
            data = data[1..];
        }
        else
            Block8x8_EmbeddedPattern(ref data, outPtr);
#if DRAW_BLOCKS
        if (blockType == TypeCopyFull && fixedPatternI <= 0)
            DrawDebugCopyBlock8x8(outPtr);
        else
            DrawDebugBlock8x8(outPtr);
#endif
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
        var pattern = Swap(ReadU64(ref data));
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
