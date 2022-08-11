namespace DigiChrome;
using System;
using System.Runtime.InteropServices;

public class Frame
{
    private readonly Decoder parent;

    internal Frame(Decoder parent) => this.parent = parent;

    public int Width => parent.colorDecoder.Width;
    public int Height => parent.colorDecoder.Height;
    public ReadOnlySpan<Color> Palette => parent.colorDecoder.Palette;
    public ReadOnlySpan<byte> Color => parent.colorDecoder.Target;
    public ReadOnlySpan<byte> Audio => parent.CurrentAudio;
    public ReadOnlySpan<sbyte> SignedAudio => MemoryMarshal.Cast<byte, sbyte>(parent.CurrentAudio);
}
