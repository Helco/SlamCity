#if DRAW_BLOCKS
namespace DigiChrome;
using System;

unsafe partial class ColorDecoder
{
    private void DrawDebugBlock(int w, int h, ReadOnlySpan<byte> inData, byte* outPtr)
    {
        for (int y = 0; y < h; y++)
        {
            inData[..w].CopyTo(new Span<byte>(outPtr, w));
            inData = inData[w..];
            outPtr += curWidth * BlockSize;
        }
    }

    private void DrawDebugBlock8x8(byte* outPtr) => DrawDebugBlock(8, 8, DebugBlock8x8, outPtr);
    private void DrawDebugCopyBlock8x8(byte* outPtr) => DrawDebugBlock(8, 8, DebugCopyBlock8x8, outPtr);
    private void DrawDebugBlock8x4(byte* outPtr) => DrawDebugBlock(8, 4, DebugBlock8x4, outPtr);
    private void DrawDebugBlock4x4(byte* outPtr) => DrawDebugBlock(4, 4, DebugBlock4x4, outPtr);

    private const byte w = 0xff;
    private static readonly byte[] DebugBlock8x8 =
    {
        0, 0, 0, 0, 0, 0, 0, 0,
        0, w, w, w, w, w, w, 0,
        0, w, w, w, w, w, w, 0,
        0, w, w, 0, 0, w, w, 0,
        0, w, w, 0, 0, w, w, 0,
        0, w, w, w, w, w, w, 0,
        0, w, w, w, w, w, w, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
    };

    private static readonly byte[] DebugCopyBlock8x8 =
    {
        0, 0, 0, 0, 0, 0, 0, 0,
        0, w, w, w, w, w, w, 0,
        0, w, w, w, w, w, w, 0,
        0, w, w, w, w, w, w, 0,
        0, w, w, w, w, w, w, 0,
        0, w, w, w, w, w, w, 0,
        0, w, w, w, w, w, w, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
    };

    private static readonly byte[] DebugBlock8x4 =
    {
        0, 0, 0, 0, 0, 0, 0, 0,
        0, w, w, w, 0, w, w, 0,
        0, w, w, 0, w, w, w, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
    };

    private static readonly byte[] DebugBlock4x4 =
    {
        0, 0, 0, 0,
        0, w, w, 0,
        0, w, w, 0,
        0, 0, 0, 0,
    };
}

#endif
