using SCI_Lib.Utils;
using System.IO;

namespace SCI_Lib.Resources.Picture
{
    public class PicHeader11
    {
        private int _offset;

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

        public static PicHeader11 Read(Stream stream)
        {
            PicHeader11 h = new();
            h.ReadStream(stream);
            return h;
        }

        private void ReadStream(Stream stream)
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

        public void Write(ByteBuilder bb)
        {
            _offset = bb.Position;
            bb.AddUShortBE(Size);      // 0
            bb.AddByte(NumPriorities); // 2
            bb.AddByte(PriBandCount);  // 3
            bb.AddByte(CelCount);      // 4
            bb.AddByte(Unknown);       // 5
            bb.AddUShortBE(XVanish);   // 6
            bb.AddUShortBE(YVanish);   // 8
            bb.AddUShortBE(ViewAngle); // 10
            bb.AddIntBE(VectorDataSize); // 12
            bb.AddIntBE(VectorDataOffset); // 16
            bb.AddIntBE(PriCelOffset);  // 20
            bb.AddIntBE(CtlCelOffset);  // 24
            bb.AddIntBE(PaletteOffset); // 28
            bb.AddIntBE(CelHeaderOffset); // 32
            bb.AddIntBE(Unknown2);  // 36
            // 40
        }

        internal void SetVectorPosition(ByteBuilder bb, int position)
        {
            bb.SetIntBE(_offset + 16, position);
        }

        internal void SetPalettePosition(ByteBuilder bb, int position)
        {
            bb.SetIntBE(_offset + 28, position);
        }

        internal void SetCellPosition(ByteBuilder bb, int position)
        {
            bb.SetIntBE(_offset + 32, position);
        }
    }
}
