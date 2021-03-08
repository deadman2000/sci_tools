using SCI_Lib.Compression;
using SCI_Lib.Decompression;
using SCI_Lib.Utils;
using System;
using System.IO;

namespace SCI_Lib.SCI1
{
    public class ResourceFileInfo1 : ResourceFileInfo
    {
        public ResourceFileInfo1(string path, int offset)
        {
            using (FileStream fs = File.OpenRead(path))
            {
                fs.Position = offset;
                ResT = (byte)fs.ReadByte();
                ResNr = fs.ReadUShortBE();
                CompSize = (ushort)(fs.ReadUShortBE() - 4);
                DecompSize = fs.ReadUShortBE();
                Method = fs.ReadUShortBE();
            }
        }

        public ResourceFileInfo1(byte resT, ushort resNum, ushort method)
        {
            ResT = resT;
            ResNr = resNum;
            CompSize = 0;
            DecompSize = 0;
            Method = method;
        }

        public override void Write(Stream stream)
        {
            stream.WriteByte(ResT);
            stream.WriteUShortBE(ResNr);
            stream.WriteUShortBE((ushort)(CompSize + 4));
            stream.WriteUShortBE(DecompSize);
            stream.WriteUShortBE(Method);
        }

        public override int HeadSize => 9;

        public override Compressor GetCompressor()
        {
            switch (Method)
            {
                case 2: return new CompressorLZW1();
                case 3: return new CompressorLZW1View();
                case 4: return new CompressorLZW1Pic();
                case 18:
                case 19:
                case 20: return new CompressorDCL();
                default: throw new NotImplementedException();
            }
        }

        public override Decompressor GetDecompressor()
        {
            switch (Method)
            {
                case 2: return new DecompressorLZW1();
                case 3: return new DecompressorLZW1View();
                case 4: return new DecompressorLZW1Pic();
                case 18:
                case 19:
                case 20: return new DecompressorDCL();
                default: throw new NotImplementedException();
            }
        }
    }
}
