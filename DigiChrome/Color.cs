namespace DigiChrome;

public struct Color
{
    public Color(ushort raw) => this.Raw = raw;

    public readonly ushort Raw;
    public byte R => (byte)((Raw >> 10) & 0b11111);
    public byte G => (byte)((Raw >> 5) & 0b11111);
    public byte B => (byte)((Raw >> 0) & 0b11111);
}
