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
            using (FileStream fs = File.OpenRead(path))
            {
                fs.Position = offset;
                var id = fs.ReadUShortBE();
                ResT = (byte)(id >> 11);
                ResNr = (ushort)(id & 0x7ff);
                CompSize = fs.ReadUShortBE();
                DecompSize = fs.ReadUShortBE();
                Method = fs.ReadUShortBE();
            }
        }

        public override int HeadSize => 8;

        public override Decompressor GetDecompressor()
        {
            switch (Method)
            {
                case 1: return new DecompressorLZW0();
                case 2: return new DecompressorHuffman();
                //case 2: return new DecompressorLZW1();
                default: throw new NotImplementedException();
            }
        }

        public override Compressor GetCompressor()
        {
            switch (Method)
            {
                case 1: return new CompressorLZW0();
                case 2: return new CompressorHuffman();
                default: throw new NotImplementedException();
            }
        }

        public override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
