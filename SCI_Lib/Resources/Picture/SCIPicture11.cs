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

        public SCIPackage Package { get; }

        public SCIPicture11(SCIPackage package, byte[] data)
        {
            Package = package;
            Read(data);
        }

        private void Read(byte[] data)
        {
            using var stream = new MemoryStream(data);
            _header = new PicHeader11(stream);
            stream.Seek(40, SeekOrigin.Begin);

            _priBars = new ushort[14];
            for (int i = 0; i < _priBars.Length; i++)
                _priBars[i] = stream.ReadUShortBE();

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
            throw new NotImplementedException();
        }

        public override Image GetBackground()
        {
            return _cell.GetImage();
        }

        public override void SetBackground(Bitmap bmp)
        {
            throw new NotImplementedException();
        }
    }
}
