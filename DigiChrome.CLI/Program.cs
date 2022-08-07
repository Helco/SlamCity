namespace DigiChrome.CLI;
using System;
using System.IO;
using System.Text;
using Decoder = DigiChrome.Decoder;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Directory.CreateDirectory("out");

        using var fileStream = new FileStream(@"C:\Users\Helco\Downloads\Slam City with Scottie Pippen (1995)\SlamCity\CD1\SLAM\open.avc", FileMode.Open, FileAccess.Read);
        using var decoder = new Decoder(fileStream);
        int frameI = 0;
        while (decoder.MoveNext())
        {
            using var outStream = new FileStream($"out/{frameI++}.ppm", FileMode.Create, FileAccess.Write);
            var frame = decoder.Current;
            var buffer = new byte[frame.Width * frame.Height * 3];
            for (int i = 0; i < frame.Color.Length; i++)
            {
                buffer[i * 3 + 0] = (byte)(frame.Palette[frame.Color[i]].R << 3);
                buffer[i * 3 + 1] = (byte)(frame.Palette[frame.Color[i]].G << 3);
                buffer[i * 3 + 2] = (byte)(frame.Palette[frame.Color[i]].B << 3);
            }

            outStream.Write(Encoding.UTF8.GetBytes($"P6\n{frame.Width} {frame.Height}\n255\n"));
            outStream.Write(buffer);
        }
    }
}
