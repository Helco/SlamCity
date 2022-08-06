using System;
using System.Text;
using System.IO;

namespace ExtractFCat
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("usage: ExtractFCat <path to fcat archive>");
                return;
            }
            using var fileStream = new FileStream(args[0], FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            if (reader.ReadUInt32() != 0x54_41_43_46)
            {
                Console.WriteLine("Invalid magic");
                return;
            }

            uint fileCount = reader.ReadUInt32();
            var files = new (string name, Range range)[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                files[i] = (
                    Encoding.ASCII.GetString(reader.ReadBytes(14)).Trim('\0'),
                    reader.ReadInt32()..reader.ReadInt32());
            }

            var outPath = Path.GetFileNameWithoutExtension(args[0]);
            Directory.CreateDirectory(outPath);
            foreach (var (name, range) in files)
            {
                var (offset, length) = range.GetOffsetAndLength((int)fileStream.Length);
                fileStream.Position = offset;
                var data = reader.ReadBytes(length);
                File.WriteAllBytes($"{outPath}/{name}", data);
            }
        }
    }
}