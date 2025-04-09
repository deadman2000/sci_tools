using SCI_Lib.Compression;
using SCI_Lib.Decompression;
using SCI_Lib.Utils;
using System;
using System.IO;

namespace SCI_Lib.SCI0
{
    public class ResourceFileInfo0 : ResourceFileInfo
    {
        public ResourceFileInfo0(string path, int offset)
        {
            using FileStream fs = File.OpenRead(path);
            fs.Position = offset;
            var id = fs.ReadUShortBE();
            ResT = (byte)(id >> 11);
            ResNr = (ushort)(id & 0x7ff);
            CompSize = fs.ReadUShortBE();
            DecompSize = fs.ReadUShortBE();
            Method = fs.ReadUShortBE();
        }

        public override int HeadSize => 8;

        public override Decompressor GetDecompressor() => Method switch
        {
            1 => new DecompressorLZW0(),
            //2 => new DecompressorHuffman(),
            2 => new DecompressorLZW1(),
            _ => throw new NotImplementedException(),
        };

        public override Compressor GetCompressor() => Method switch
        {
            1 => new CompressorLZW0(),
            2 => new CompressorHuffman(),
            _ => throw new NotImplementedException(),
        };

        public override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
