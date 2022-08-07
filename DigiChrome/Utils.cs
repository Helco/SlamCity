namespace DigiChrome;
using System;

internal static class Utils
{
    public static ushort SwapToBE(ushort val) => BitConverter.IsLittleEndian ? Swap(val) : val;
    public static uint SwapToBE(uint val) => BitConverter.IsLittleEndian ? Swap(val) : val;
    public static ulong SwapToBE(ulong val) => BitConverter.IsLittleEndian ? Swap(val) : val;

    public static ushort Swap(ushort val) => unchecked((ushort)((val >> 8) | (val << 8)));

    public static uint Swap(uint val)
    {
        val = (val >> 16) | (val << 16);
        val = ((val & 0xFF00_FF00) >> 8) | ((val & 0x00FF_00FF) << 8);
        return val;
    }

    public static ulong Swap(ulong val)
    {
        val = (val >> 32) | (val << 32);
        val = ((val & 0xFFFF0000_FFFF0000) >> 16) | ((val & 0x0000FFFF_0000FFFF) << 16);
        val = ((val & 0xFF00_FF00_FF00_FF00) >> 8) | ((val & 0x00FF_00FF_00FF_00FF) << 8);
        return val;
    }

    public static int PopHighestBit(ref ushort val)
    {
        int bit = (val & 0x8000) > 0 ? 1 : 0;
        val = unchecked((ushort)(val << 1));
        return bit;
    }

    public static int PopHighestBit(ref uint val)
    {
        int bit = (val & 0x8000_0000) > 0 ? 1 : 0;
        val = unchecked(val << 1);
        return bit;
    }

    public static int PopHighestBit(ref ulong val)
    {
        int bit = (val & 0x8000_0000_0000_0000) > 0 ? 1 : 0;
        val = unchecked(val << 1);
        return bit;
    }

    public static int PopLowestBit(ref ushort val)
    {
        int bit = (val & 1) > 0 ? 1 : 0;
        val = unchecked((ushort)(val >> 1));
        return bit;
    }

    public static int PopLowestBit(ref uint val)
    {
        int bit = (val & 1) > 0 ? 1 : 0;
        val = unchecked(val >> 1);
        return bit;
    }

    public static int PopLowestBit(ref ulong val)
    {
        int bit = (val & 1) > 0 ? 1 : 0;
        val = unchecked(val >> 1);
        return bit;
    }
}
