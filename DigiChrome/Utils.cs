namespace DigiChrome;
using System;
using System.Runtime.InteropServices;

internal static class Utils
{
    public static byte ReadU8(ref ReadOnlySpan<byte> data) => Read<byte>(ref data);
    public static ushort ReadU16(ref ReadOnlySpan<byte> data) => SwapFromLE(Read<ushort>(ref data));
    public static uint ReadU32(ref ReadOnlySpan<byte> data) => SwapFromLE(Read<uint>(ref data));
    public static ulong ReadU64(ref ReadOnlySpan<byte> data) => SwapFromLE(Read<ulong>(ref data));

    private static unsafe T Read<T>(ref ReadOnlySpan<byte> data) where T : unmanaged
    {
        var result = MemoryMarshal.Cast<byte, T>(data)[0];
        data = data[sizeof(T)..];
        return result;
    }

    public static ushort SwapFromLE(ushort val) => BitConverter.IsLittleEndian ? val : Swap(val);
    public static uint SwapFromLE(uint val) => BitConverter.IsLittleEndian ? val : Swap(val);
    public static ulong SwapFromLE(ulong val) => BitConverter.IsLittleEndian ? val : Swap(val);

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
}
