namespace DigiChrome;
using System;
using static DigiChrome.Utils;

unsafe partial class ColorDecoder
{
    private void Block4x4(int x, int y, ref ReadOnlySpan<byte> data, byte* outPtr)
    {
        if (data[0] == TypeCopyHalf)
        {
            Block_Copy(x, y, BlockSize / 2, BlockSize / 2, outPtr);
            data = data[1..];
        }
        else
            Block4x4_Pattern(ref data, outPtr);
#if DRAW_BLOCKS
        DrawDebugBlock4x4(outPtr);
#endif
    }

    private void Block4x4_Pattern(ref ReadOnlySpan<byte> data, byte* outPtr)
    {
        var offsets = ColorOffset4x4(Indices[data[0]]);
        var colors = GetColors(ref data);
        var pattern = ReadU16(ref data);

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
                outPtr[j] = colors[offsets[j / 2] + PopLowestBit(ref pattern)];
            outPtr += curWidth * BlockSize;
            offsets = offsets[2..];
        }
    }
}
