namespace DigiChrome;
using System;
using static DigiChrome.Utils;

unsafe partial class ColorDecoder
{
    private void Block4x4(int x, int y, ref ReadOnlySpan<byte> data, byte* outPtr)
    {
        if (data[0] == TypeCopyHalf)
            Block_Copy(x, y, BlockSize / 2, BlockSize / 2, outPtr);
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
        var pattern = SwapToLE(BitConverter.ToUInt16(data));
        data = data[sizeof(ushort)..];

        outPtr += 3 * curWidth * BlockSize;
        for (int i = 0; i < 4; i++)
        {
            outPtr[3] = colors[offsets[^1] + PopHighestBit(ref pattern)];
            outPtr[2] = colors[offsets[^1] + PopHighestBit(ref pattern)];
            outPtr[1] = colors[offsets[^2] + PopHighestBit(ref pattern)];
            outPtr[0] = colors[offsets[^2] + PopHighestBit(ref pattern)];
            outPtr -= curWidth * BlockSize;
            offsets = offsets[..^2];
        }
    }
}
