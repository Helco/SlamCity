namespace DigiChrome;
using System;
using static DigiChrome.Utils;

unsafe partial class ColorDecoder
{
    private void Block8x4(int x, int y, ref ReadOnlySpan<byte> data, byte* outPtr)
    {
        if (data[0] == TypeCopyHalf)
        {
            Block_Copy(x, y, BlockSize, BlockSize / 2, outPtr);
            data = data[1..];
        }
        else
            Block8x4_Pattern(ref data, outPtr);
#if DRAW_BLOCKS
        DrawDebugBlock8x4(outPtr);
#endif
    }

    private void Block8x4_Pattern(ref ReadOnlySpan<byte> data, byte* outPtr)
    {
        var offsets = ColorOffset8x4(Indices[data[0]]);
        var colors = GetColors(ref data);
        var pattern = ReadU32(ref data);

        outPtr += 3 * curWidth * BlockSize;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                outPtr[7] = colors[offsets[^1] + PopHighestBit(ref pattern)];
                outPtr[6] = colors[offsets[^1] + PopHighestBit(ref pattern)];
                outPtr[5] = colors[offsets[^2] + PopHighestBit(ref pattern)];
                outPtr[4] = colors[offsets[^2] + PopHighestBit(ref pattern)];
                outPtr[3] = colors[offsets[^3] + PopHighestBit(ref pattern)];
                outPtr[2] = colors[offsets[^3] + PopHighestBit(ref pattern)];
                outPtr[1] = colors[offsets[^4] + PopHighestBit(ref pattern)];
                outPtr[0] = colors[offsets[^4] + PopHighestBit(ref pattern)];
                outPtr -= curWidth * BlockSize;
            }
            offsets = offsets[..^4];
        }
    }
}
