using SCI_Lib.Resources.Picture;
using SCI_Lib.Utils;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace SCI_Lib.Resources.View
{
    public class Cell
    {
        public Cell(SCIPackage package, Palette pal)
        {
            Package = package;
            Palette = pal;
        }

        public Palette Palette { get; set; }

        public SCIPackage Package { get; }

        public ushort Width { get; set; }

        public ushort Height { get; set; }

        public short X { get; set; }

        public short Y { get; set; }

        public byte TransparentColor { get; set; }

        public uint PaletteOffset { get; internal set; }

        public byte[] Pixels { get; set; }

        public Image GetImage()
        {
            Palette palette = Palette != null ? Palette : Package.GlobalPalette;

            var bmp = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
            var pal = bmp.Palette;
            for (int i = 0; i < palette.Colors.Length; i++)
            {
                var col = palette.Colors[i];
                if (col.IsEmpty) col = Package.GlobalPalette.Colors[palette.ColStart + i];

                pal.Entries[palette.ColStart + i] = col;
            }

            if (palette.ColStart > 0)
            {
                for (int i = 0; i < palette.ColStart; i++)
                    pal.Entries[i] = Package.GlobalPalette.Colors[i];
            }

            bmp.Palette = pal;

            var data = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            IntPtr scan0 = data.Scan0;
            for (int y = 0; y < Height; y++)
            {
                Marshal.Copy(Pixels, y * Width, scan0, Width);
                scan0 += data.Stride;
            }

            bmp.UnlockBits(data);

            return bmp;
        }

        public void Write(ByteBuilder bbRLE, ByteBuilder bbLiterals)
        {
            ImageEncoder.WriteImage(bbRLE, bbLiterals, Pixels, Width, TransparentColor);
        }

        public void ReadVGA(Stream stream)
        {
            Width = stream.ReadUShortBE();
            Height = stream.ReadUShortBE();
            X = stream.ReadB();
            Y = stream.ReadB();
            TransparentColor = stream.ReadB();
            stream.Position++; // Skip unknown

            Pixels = new byte[Width * Height];
            ImageEncoder.ReadImage(stream, stream, Pixels, TransparentColor);
        }

        public void ReadVGA11(byte[] data, int offset)
        {
            var ms = new MemoryStream(data);
            ms.Seek(offset, SeekOrigin.Begin);

            Width = ms.ReadUShortBE();
            Height = ms.ReadUShortBE();
            X = ms.ReadShortBE();
            Y = ms.ReadShortBE();
            TransparentColor = ms.ReadB();

            var always_0xa = ms.ReadB();
            var temp2 = ms.ReadB();
            var temp3 = ms.ReadB();
            var totalCellDataSize = ms.ReadUIntBE();
            var rleCellDataSize = ms.ReadUIntBE();
            PaletteOffset = ms.ReadUIntBE();
            var offsetRLE = ms.ReadUIntBE();
            if (offsetRLE == 0) throw new FormatException();
            var offsetLiteral = ms.ReadUIntBE();
            var perRowOffsets = ms.ReadUIntBE();
            if (always_0xa == 0xa || always_0xa == 0x8a)
            {
                if (offsetLiteral == 0) throw new FormatException();
            }
            else
            {
                if (offsetLiteral != 0) throw new FormatException();
            }

            ms.Position = offsetRLE;
            if (offsetLiteral == 0)
            {
                Pixels = ms.ReadBytes(Width * Height);
            }
            else
            {
                ms.Position = offsetRLE;
                using var msLiteral = new MemoryStream(data);
                msLiteral.Seek(offsetLiteral, SeekOrigin.Begin);
                Pixels = new byte[Width * Height];
                ImageEncoder.ReadImage(ms, msLiteral, Pixels, TransparentColor);
            }
        }
    }
}
