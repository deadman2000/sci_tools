using SCI_Lib.Utils;
using System;
using System.IO;

namespace SCI_Lib.Resources.Picture
{
    public class PicImage : PicExtCommand
    {
        private PointShort _coord;
        private byte _transpCol;

        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public byte[] Image { get; set; }

        public PicImage(Stream stream) : base(0x01)
        {
            _coord = stream.ReadPicAbsCoord();
            ushort size = stream.ReadUShortBE();
            var startPos = stream.Position;

            Width = stream.ReadUShortBE();
            Height = stream.ReadUShortBE();
            stream.Seek(2, SeekOrigin.Current);
            _transpCol = stream.ReadB();
            stream.Seek(1, SeekOrigin.Current);

            Image = new byte[Width * Height];

            ImageEncoder.ReadImage(stream, stream, Image, _transpCol, true);

            var readSize = stream.Position - startPos;

            if (size != readSize)
                throw new FormatException();
        }

        protected override void WriteExt(ByteBuilder bb)
        {
            bb.WritePicAbsCoord(ref _coord);
            var sizePos = bb.Position;
            bb.AddShortBE(0); // Потом вернемся, чтобы записать размер данных

            bb.AddUShortBE(Width);
            bb.AddUShortBE(Height);
            bb.AddShortBE(0);
            bb.AddByte(_transpCol);
            bb.AddByte(0);
            ImageEncoder.WriteImage(bb, bb, Image, Width, _transpCol);

            var endPos = bb.Position;
            var size = endPos - sizePos - 2;
            if (size > 0xffff)
                throw new FormatException("Too big image data");

            bb.SetUShortBE(sizePos, (ushort)size);
        }

        public byte[] GetBytes()
        {
            ByteBuilder bb = new ByteBuilder();
            WriteExt(bb);
            return bb.GetArray();
        }
    }
}
