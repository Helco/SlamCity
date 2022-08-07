namespace DigiChrome;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;


[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct ColorHeader
{
    public Section section;
    public uint unknown1;
    public byte unknown2;
    public byte colorCount;
    public byte width;
    public byte height;
}

internal unsafe partial class ColorDecoder
{
    internal int Width => curWidth * BlockSize;
    internal int Height => curHeight * BlockSize;
    internal readonly Color[] Palette = new Color[byte.MaxValue + 1];
    internal ReadOnlySpan<byte> Target => targets[curTarget];
    private Span<byte> PrevTarget => targets[(curTarget + 1) % targets.Length];

    private byte[][] targets = { Array.Empty<byte>(), Array.Empty<byte>() };
    private int prevWidth, prevHeight, curWidth, curHeight;
    private int curTarget;

    public void Decode(ReadOnlySpan<byte> data)
    {
        curTarget = (curTarget + 1) % targets.Length;
        (prevWidth, prevHeight) = (curWidth, curHeight);

        var header = Header(ref data);
        PaletteColors(ref data, header.colorCount);
        Blocks(ref data);
    }

    private ColorHeader Header(ref ReadOnlySpan<byte> data)
    {
        if (data.Length < sizeof(ColorHeader))
            throw new EndOfStreamException("Color packet is not large enough for header");
        var header = MemoryMarshal.Cast<byte, ColorHeader>(data)[0];
        
        var pixelSize = header.width * header.height * BlockSizeSqr;
        if (Target.Length < pixelSize)
            targets[curTarget] = new byte[pixelSize];

        (curWidth, curHeight) = (header.width, header.height);

        data = data[sizeof(ColorHeader)..];
        return header;
    }

    private void PaletteColors(ref ReadOnlySpan<byte> data, int count)
    {
        var paletteSize = count * sizeof(Color);
        if (data.Length < paletteSize)
            throw new EndOfStreamException("Color packet is not large enough for palette");
        data[..paletteSize].CopyTo(MemoryMarshal.AsBytes<Color>(Palette));
        data = data[paletteSize..];
    }

    private void Blocks(ref ReadOnlySpan<byte> data)
    {
        int repeatCount = 0;
        ReadOnlySpan<byte> repeatStart = data;

        fixed (byte* outStartPtr = targets[curTarget])
        {
            byte* outRowPtr = outStartPtr;
            for (int y = 0; y < curHeight; y++, outRowPtr += curWidth * BlockSizeSqr)
            {
                byte* outPtr = outRowPtr;
                for (int x = 0; x < curWidth; x++, outPtr += BlockSize)
                {
                    if (repeatCount > 0)
                    {
                        repeatCount--;
                        data = repeatStart;
                    }
                    else if ((data[0] & 0x80) > 0)
                    {
                        repeatCount = (data[0] & 0x7F) + 1;
                        repeatStart = data = data[1..];
                    }

                    Block(x, y, ref data, outPtr);
                }
            }
        }
    }

    private void Block(int x, int y, ref ReadOnlySpan<byte> data, byte* outPtr)
    {
        byte category = Categories[data[0]];
        if (category == Category8x8)
        {
            Block8x8(x, y, ref data, outPtr);
            return;
        }

        if (category == Category8x4)
            Block8x4(x, y, ref data, outPtr);
        else
        {
            Block4x4(x, y, ref data, outPtr);
            Block4x4(x, y, ref data, outPtr + BlockSize / 2);
        }

        outPtr += curWidth * BlockSizeSqr / 2;
        category = Categories[data[0]];
        if (category == Category8x4)
            Block8x4(x, y, ref data, outPtr);
        else
        {
            Block4x4(x, y, ref data, outPtr);
            Block4x4(x, y, ref data, outPtr + BlockSize / 2);
        }
    }

    private static ReadOnlySpan<byte> GetColors(ref ReadOnlySpan<byte> data)
    {
        var colorCount = 2 * ColorPairCount[data[0]];
        var colors = data[1..(colorCount + 1)];
        data = data[(colorCount + 1)..];
        return colors;
    }

    private void Block_Copy(int dstX, int dstY, int copyWidth, int copyHeight, byte* outPtr)
    {
        // In Slam City all copy blocks can be ignored due to some clipping rect
        // but it seems like this was for rendering, not mandated by the video content itself.
        // So we ignore it and just care about different frame sizes
        // Reminder: videos are always centered on screen

        var (srcX, dstOffX) = CopyClip(dstX, curWidth, prevWidth, ref copyWidth);
        var (srcY, dstOffY) = CopyClip(dstY, curHeight, prevHeight, ref copyHeight);
        if (copyWidth <= 0 || copyHeight <= 0)
            return;

        fixed (byte* inStartPtr = PrevTarget)
        {
            outPtr += dstOffX + dstOffY * curWidth * BlockSize;
            var inPtr = inStartPtr + srcX + srcY * prevWidth * BlockSize;
            for (int i = 0; i < copyHeight; i++)
            {
                Buffer.MemoryCopy(inPtr, outPtr, copyWidth, copyWidth);
                outPtr += curWidth * BlockSize;
                inPtr += prevWidth * BlockSize;
            }
        }
    }

    private (int src, int dstOff) CopyClip(int dstBlock, int dstBlockSize, int srcBlockSize, ref int copySize)
    {
        int dst = dstBlock * BlockSize; // work in pixels to account for odd block size
        int dstSize = dstBlockSize * BlockSize;
        int srcSize = srcBlockSize * BlockSize;
        int dstOff = 0;
        int src = dst + (srcSize - dstSize) / 2;
        if (src < 0)
        {
            dstOff = -src;
            copySize += src;
            src = 0;
        }
        copySize = Math.Min(copySize, srcSize - src);
        return (src, dstOff);
    }
}
