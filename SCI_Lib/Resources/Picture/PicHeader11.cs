using SCI_Lib.Utils;
using System.IO;

namespace SCI_Lib.Resources.Picture
{
    public class PicHeader11
    {
        public ushort Size { get; set; }
        public byte NumPriorities { get; set; }
        public byte PriBandCount { get; set; }
        public byte CelCount { get; set; }
        public byte Unknown { get; set; }
        public ushort XVanish { get; set; }
        public ushort YVanish { get; set; }
        public ushort ViewAngle { get; set; }
        public uint VectorDataSize { get; set; }
        public uint VectorDataOffset { get; set; }
        public uint PriCelOffset { get; set; }
        public uint CtlCelOffset { get; set; }
        public uint PaletteOffset { get; set; }
        public uint CelHeaderOffset { get; set; }
        public uint Unknown2 { get; set; }

        public PicHeader11(Stream stream)
        {
            Size = stream.ReadUShortBE();
            NumPriorities = stream.ReadB();
            PriBandCount = stream.ReadB();
            CelCount = stream.ReadB();
            Unknown = stream.ReadB();
            XVanish = stream.ReadUShortBE();
            YVanish = stream.ReadUShortBE();
            ViewAngle = stream.ReadUShortBE();
            VectorDataSize = stream.ReadUIntBE();
            VectorDataOffset = stream.ReadUIntBE();
            PriCelOffset = stream.ReadUIntBE();
            CtlCelOffset = stream.ReadUIntBE();
            PaletteOffset = stream.ReadUIntBE();
            CelHeaderOffset = stream.ReadUIntBE();
            Unknown2 = stream.ReadUIntBE();
        }

    }
}
