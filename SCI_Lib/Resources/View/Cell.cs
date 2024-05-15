using SCI_Lib.Resources.Picture;
using SCI_Lib.Utils;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace SCI_Lib.Resources.View
{
    public class Cell
    {
        private byte _always_0xa;

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

        public bool NoRLE { get; set; }

        public PointByte[] Bones { get; set; }

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

        public void ReadEVGA(Stream stream, bool isVGA, int boneCount)
        {
            Width = stream.ReadUShortBE();
            Height = stream.ReadUShortBE();
            X = stream.ReadB();
            Y = stream.ReadB();
            TransparentColor = stream.ReadB();

            if (isVGA)
                stream.Position++; // Skip unknown

            if (boneCount > 0)
            {
                Bones = new PointByte[boneCount];
                for (int i = 0; i < boneCount; i++)
                {
                    Bones[i] = stream.ReadPointByte();
                }
            }

            Pixels = new byte[Width * Height];
            ImageEncoder.ReadImage(stream, stream, Pixels, TransparentColor, isVGA);
        }

        public void WriteEVGA(ByteBuilder bb, bool isVGA)
        {
            bb.AddUShortBE(Width);
            bb.AddUShortBE(Height);
            bb.AddByte((byte)X);
            bb.AddByte((byte)Y);
            bb.AddByte(TransparentColor);

            if (isVGA)
                bb.AddByte(0); // Unknown

            ImageEncoder.WriteImage(bb, bb, Pixels, Width, TransparentColor);
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

            _always_0xa = ms.ReadB();
            var temp2 = ms.ReadB();
            var temp3 = ms.ReadB();
            var totalCellDataSize = ms.ReadUIntBE();
            var rleCellDataSize = ms.ReadUIntBE();
            PaletteOffset = ms.ReadUIntBE();
            var offsetRLE = ms.ReadUIntBE();
            if (offsetRLE == 0) throw new FormatException();
            var offsetLiteral = ms.ReadUIntBE();
            var perRowOffsets = ms.ReadUIntBE();
            if (_always_0xa == 0xa || _always_0xa == 0x8a)
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
                NoRLE = true;
                Pixels = ms.ReadBytes(Width * Height);
            }
            else
            {
                NoRLE = false;
                ms.Position = offsetRLE;
                using var msLiteral = new MemoryStream(data);
                msLiteral.Seek(offsetLiteral, SeekOrigin.Begin);
                Pixels = new byte[Width * Height];
                ImageEncoder.ReadImage(ms, msLiteral, Pixels, TransparentColor, true);
            }
        }

        public void WriteVGA11(ByteBuilder bb)
        {
            bb.AddUShortBE(Width);
            bb.AddUShortBE(Height);
            bb.AddShortBE(X);
            bb.AddShortBE(Y);
            bb.AddByte(TransparentColor);
            bb.AddByte((byte)(NoRLE ? 0 : 0x0a));
            bb.AddShortBE(0);

            if (NoRLE)
            {
                bb.AddIntBE(Pixels.Length); // Total size
                bb.AddIntBE(0); // RLE size

                int offsetsPos = bb.Position;
                bb.AddIntBE(0); // Palette offset
                bb.AddIntBE(0); // RLE offset
                bb.AddIntBE(0); // Literals offset
                bb.AddIntBE(0); // Per row offset

                bb.AddBytes(Pixels);

                // Palette
                if (Palette != null)
                {
                    bb.SetIntBE(offsetsPos, bb.Position);
                    Palette.Write(bb);
                }
            }
            else
            {
                var bbRLE = new ByteBuilder();
                var bbLit = new ByteBuilder();

                Write(bbRLE, bbLit);
                var rleData = bbRLE.GetArray();
                var litData = bbLit.GetArray();

                bb.AddIntBE(rleData.Length + litData.Length); // Total size
                bb.AddIntBE(rleData.Length); // RLE size

                int offsetsPos = bb.Position;
                bb.AddIntBE(0); // Palette offset
                bb.AddIntBE(0); // RLE offset
                bb.AddIntBE(0); // Literals offset
                bb.AddIntBE(0); // Per row offset

                // RLE
                bb.SetIntBE(offsetsPos + 4, bb.Position);
                bb.AddBytes(rleData);

                // Literals
                bb.SetIntBE(offsetsPos + 8, bb.Position);
                bb.AddBytes(litData);

                // Palette
                if (Palette != null)
                {
                    bb.SetIntBE(offsetsPos, bb.Position);
                    Palette.Write(bb);
                }
            }
        }

        public void SetImage(Bitmap bitmap)
        {
            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                    Pixels[x + y * Width] = Palette.GetColorIndex(bitmap.GetPixel(x, y));
        }

        public void SetImageIndexed(Bitmap bmp)
        {
            if (bmp.Width != Width || bmp.Height != Height)
                throw new ArgumentException("Different image size");

            if (bmp.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new ArgumentException($"Wrong image pixel format {bmp.PixelFormat}");

            var data = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            var scan0 = data.Scan0;
            Marshal.Copy(scan0, Pixels, 0, Pixels.Length);

            bmp.UnlockBits(data);
        }
    }
}
