using SCI_Lib.Compression;
using SCI_Lib.Decompression;
using System.IO;

namespace SCI_Lib
{
    public abstract class ResourceFileInfo
    {
        public byte ResT { get; protected set; }

        public ushort ResNr { get; protected set; }

        public ushort CompSize { get; set; }

        public ushort DecompSize { get; set; }

        public ushort Method { get; protected set; }

        public abstract int HeadSize { get; }

        public override string ToString() => $"res_t:{ResT:X2} res_nr:{ResNr} comp_size:{CompSize} decomp_size:{DecompSize} method:{Method}";

        public abstract Decompressor GetDecompressor();

        public abstract Compressor GetCompressor();

        public abstract void Write(Stream stream);

    }
}
