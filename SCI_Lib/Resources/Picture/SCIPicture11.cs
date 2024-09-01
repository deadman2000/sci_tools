using SCI_Lib.Resources.View;
using SCI_Lib.Utils;
using System;
using System.Drawing;
using System.IO;

namespace SCI_Lib.Resources.Picture
{
    internal class SCIPicture11 : SCIPicture
    {
        private PicHeader11 _header;
        private ushort[] _priBars;
        private Palette _palette;
        private Cell _cell;
        private PicVector _vector;

        public SCIPackage Package { get; }

        public SCIPicture11(SCIPackage package, byte[] data)
        {
            Package = package;
            Read(data);
        }

        public override int Width => _cell.Width;

        public override int Height => _cell.Height;

        private void Read(byte[] data)
        {
            using var stream = new MemoryStream(data);
            _header = PicHeader11.Read(stream);

            if (stream.Position != 40) throw new FormatException();
            //stream.Seek(40, SeekOrigin.Begin);

            _priBars = new ushort[_header.PriBandCount];
            for (int i = 0; i < _priBars.Length; i++)
                _priBars[i] = stream.ReadUShortBE();

            if (_header.CtlCelOffset > 0)
            {
                throw new NotImplementedException();
            }

            if (_header.VectorDataOffset > 0)
            {
                stream.Seek(_header.VectorDataOffset, SeekOrigin.Begin);
                _vector = PicVector.Read(stream);
            }

            if (_header.PaletteOffset > 0)
            {
                stream.Seek(_header.PaletteOffset, SeekOrigin.Begin);
                _palette = Palette.Read(stream);
            }

            if (_header.CelCount > 0)
            {
                _cell = new Cell(Package, _palette);
                _cell.ReadVGA11(data, (int)_header.CelHeaderOffset);
            }
        }

        public override byte[] GetBytes()
        {
            ByteBuilder bb = new();

            _header.Write(bb);
            for (int i = 0; i < _header.PriBandCount; i++)
                bb.AddUShortBE(_priBars[i]);

            if (_cell != null)
            {
                _header.SetCellPosition(bb, bb.Position);
                _cell.WriteVGA11(bb);
            }

            if (_palette != null)
            {
                _header.SetPalettePosition(bb, bb.Position);
                _palette.Write(bb);
            }

            if (_vector != null)
            {
                _header.SetVectorPosition(bb, bb.Position);
                _vector.Write(bb);
            }

            return bb.GetArray();
        }

        public override Image GetBackground()
        {
            return _cell.GetImage();
        }

        public override void SetBackground(Bitmap bmp)
        {
            _cell.SetImage(bmp);
        }

        public override void SetBackgroundIndexed(Bitmap bmp)
        {
            _cell.SetImageIndexed(bmp);
        }

        public override Color[] GetPalette()
        {
            return _palette.Colors;
        }

        public override byte[] GetPixels()
        {
            return _cell.Pixels;
        }

        public override void SetPixels(byte[] pixels)
        {
            _cell.Pixels = pixels;
        }
    }
}
