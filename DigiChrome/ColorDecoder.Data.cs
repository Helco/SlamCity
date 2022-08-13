﻿namespace DigiChrome;
using System;

partial class ColorDecoder
{
    private const int BlockSize = 8;
    private const int BlockSizeSqr = BlockSize * BlockSize;
    private const byte Category8x8 = 4;
    private const byte Category8x4 = 2;
    private const byte TypeCopyFull = 91;
    private const byte TypeCopyHalf = 92;

    // category 0 and 1 are treated identically
    private static readonly byte[] Categories =
    {
        0,   4,   0,   4,   4,   4,   4,   4,
        2,   1,   1,   1,   4,   4,   4,   4,
        2,   2,   1,   1,   1,   2,   1,   2,
        1,   1,   2,   2,   1,   2,   2,   2,
        2,   2,   2,   2,   2,   2,   2,   2,
        2,   2,   2,   2,   2,   2,   2,   2,
        1,   1,   1,   1,   1,   1,   1,   1,
        1,   1,   1,   1,   1,   1,   1,   1,
        4,   4,   4,   4,   4,   4,   4,   4,
        4,   4,   4,   4,   4,   4,   4,   4,
        4,   4,   4,   4,   4,   4,   4,   4,
        4,   4,   4,   4,   2,   1,   0,   0,
        0,   0,   0,   0,   0,   0,   0,   0,
        0,   0,   0,   0,   0,   0,   0,   0,
        0,   0,   0,   0,   0,   0,   0,   0,
        0,   0,   0,   0,   0,   0,   0,   0
    };

    // Depending on the block category this is an index to a fixed pattern or a color offset
    private static readonly byte[] Indices =
    {
        0,   0,   0,   1,   2,   3,   4,   5,
        0,   0,   1,   2,   6,   7,   8,   9,
        1,   2,   3,   4,   5,   3,   6,   4,
        7,   8,   5,   6,   9,   7,   8,   9,
       10,  11,  12,  13,  14,  15,  16,  17,
       18,  19,  20,  21,  22,  23,  24,  25,
       10,  11,  12,  13,  14,  15,  16,  17,
       18,  19,  20,  21,  22,  23,  24,  25,
        2,   3,   4,   5,   6,   7,   8,   9,
       10,  11,  12,  13,  14,  15,  16,  17,
       18,  19,  20,  21,  22,  23,  24,  25,
        1,   1,   1,   0,   0,   0,   0,   0,
        0,   1,   0,   1,   1,   1,   1,   1,
        1,   4,   4,   5,   1,   1,   1,   1,
        2,   3,   3,   4,   4,   5,   5,   4,
        3,   2,   5,   4,   1,   4,   4,   3
    };

    private static readonly byte[] ColorPairCount =
    {
        0,   1,   0,   1,   1,   1,   1,   1,
        1,   4,   4,   5,   1,   1,   1,   1,
        2,   3,   3,   4,   4,   5,   5,   4,
        3,   2,   5,   4,   1,   4,   4,   3,
        8,   7,   7,   6,   7,   6,   6,   5,
        7,   6,   6,   5,   6,   5,   5,   4,
        8,   7,   7,   6,   7,   6,   6,   5,
        7,   6,   6,   5,   6,   5,   5,   4,
        1,   1,   1,   1,   1,   1,   1,   1,
        1,   1,   1,   1,   1,   1,   1,   1,
        1,   1,   1,   1,   1,   1,   1,   1,
        1,   1,   1,   0,   0,   0,   0,   0
    };

    private static readonly ulong[] FixedPatterns =
    {
        0xAA_55_AA_55_AA_55_AA_55,
        0x00_00_AA_55_AA_55_FF_FF,
        0x0A_05_2A_15_AB_57_AF_5F,
        0x2B_17_2B_17_2B_17_2B_17,
        0xAF_57_AB_55_2A_55_2A_05,
        0xFF_FF_AA_55_AA_55_00_00,
        0xFA_F5_EA_D5_A8_54_A0_50,
        0xE8_D4_E8_D4_E8_D4_E8_D4,
        0xA0_54_AA_54_AA_D5_EA_F5,
        0x00_00_00_00_AA_55_AA_55,
        0x00_01_02_05_0A_15_2A_55,
        0x0A_05_0A_05_0A_05_0A_05,
        0x2A_55_0A_15_02_05_00_01,
        0xAA_55_AA_55_00_00_00_00,
        0xAA_54_A8_50_A0_40_80_00,
        0xA0_50_A0_50_A0_50_A0_50,
        0x80_00_A0_40_A8_50_AA_54,
        0xAA_55_AA_55_FF_FF_FF_FF,
        0xAB_55_AF_57_BF_5F_FF_7F,
        0xAF_5F_AF_5F_AF_5F_AF_5F,
        0xFF_7F_BF_5F_AF_57_AB_55,
        0xFF_FF_FF_FF_AA_55_AA_55,
        0xFE_FF_FA_FD_EA_F5_AA_D5,
        0xFA_F5_FA_F5_FA_F5_FA_F5,
        0xAA_D5_EA_F5_FA_FD_FE_FF
    };

    private static ReadOnlySpan<byte> ColorOffset8x4(int index) => allColorOffsets8x4.AsSpan(index * 8, 8);
    private static readonly byte[] allColorOffsets8x4 =
    {
        0,   0,   0,   0,   0,   0,   0,   0,
        0,   0,   2,   2,   0,   0,   2,   2,
        0,   0,   2,   2,   0,   0,   4,   4,
        0,   2,   4,   4,   6,   8,   4,   4,
        0,   2,   4,   4,   6,   6,   4,   4,
        0,   0,   2,   4,   0,   0,   6,   8,
        0,   0,   2,   4,   0,   0,   6,   6,
        0,   0,   2,   2,   4,   6,   2,   2,
        0,   0,   2,   2,   0,   0,   4,   6,
        0,   0,   2,   2,   4,   4,   2,   2,
        0,   2,   4,   6,   8,  10,  12,  14,
        0,   2,   4,   6,   8,  10,  12,  12,
        0,   2,   4,   6,   8,   8,  10,  12,
        0,   2,   4,   6,   8,   8,  10,  10,
        0,   2,   4,   4,   6,   8,  10,  12,
        0,   2,   4,   4,   6,   8,  10,  10,
        0,   2,   4,   4,   6,   6,   8,  10,
        0,   2,   4,   4,   6,   6,   8,   8,
        0,   0,   2,   4,   6,   8,  10,  12,
        0,   0,   2,   4,   6,   8,  10,  10,
        0,   0,   2,   4,   6,   6,   8,  10,
        0,   0,   2,   4,   6,   6,   8,   8,
        0,   0,   2,   2,   4,   6,   8,  10,
        0,   0,   2,   2,   4,   6,   8,   8,
        0,   0,   2,   2,   4,   4,   6,   8,
        0,   0,   2,   2,   4,   4,   6,   6
    };

    private static ReadOnlySpan<byte> ColorOffset4x4(int index) => allColorOffsets4x4.AsSpan(index * 8, 8);
    private static readonly byte[] allColorOffsets4x4 =
    {
        0,   2,   4,   2,   6,   6,   6,   6,
        0,   2,   0,   4,   6,   6,   6,   6,
        0,   2,   4,   6,   8,   8,   8,   8,
        0,   2,   0,   2,   4,   4,   4,   4,
        0,   0,   0,   0,   2,   4,   6,   4,
        0,   0,   0,   0,   2,   4,   2,   6,
        0,   0,   0,   0,   2,   4,   6,   8,
        0,   0,   0,   0,   2,   4,   2,   4,
        0,   0,   0,   0,   2,   2,   2,   2,
        0,   0,   0,   0,   0,   0,   0,   0,
        0,   2,   4,   6,   8,  10,  12,  14,
        0,   2,   4,   6,   8,  10,  12,  10,
        0,   2,   4,   6,   8,  10,   8,  12,
        0,   2,   4,   6,   8,  10,   8,  10,
        0,   2,   4,   2,   6,   8,  10,  12,
        0,   2,   4,   2,   6,   8,  10,   8,
        0,   2,   4,   2,   6,   8,   6,  10,
        0,   2,   4,   2,   6,   8,   6,   8,
        0,   2,   0,   4,   6,   8,  10,  12,
        0,   2,   0,   4,   6,   8,  10,   8,
        0,   2,   0,   4,   6,   8,   6,  10,
        0,   2,   0,   4,   6,   8,   6,   8,
        0,   2,   0,   2,   4,   6,   8,  10,
        0,   2,   0,   2,   4,   6,   8,   6,
        0,   2,   0,   2,   4,   6,   4,   8,
        0,   2,   0,   2,   4,   6,   4,   6
    };
}
