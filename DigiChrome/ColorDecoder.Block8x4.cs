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

        for (int i = 0; i < 2; i++)
        {
            for (int k = 0; k < 2; k++)
            {
                for (int j = 0; j < BlockSize; j++)
                    outPtr[j] = colors[offsets[j / 2] + PopLowestBit(ref pattern)];
                outPtr += curWidth * BlockSize;
            }
            offsets = offsets[4..];
        }
    }
}
