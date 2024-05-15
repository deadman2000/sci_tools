using SCI_Lib.Utils;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SCI_Lib.Resources.View
{
    public class Palette
    {
        public static readonly Palette EGA = new()
        {
            Colors = new Color[16]
            {
                Color.FromArgb(0x00, 0x00, 0x00),
                Color.FromArgb(0x00, 0x00, 0xA0),
                Color.FromArgb(0x00, 0xA0, 0x00),
                Color.FromArgb(0x00, 0xA0, 0xA0),
                Color.FromArgb(0xA0, 0x00, 0x00),
                Color.FromArgb(0xA0, 0x00, 0xA0),
                Color.FromArgb(0xA0, 0x50, 0x00),
                Color.FromArgb(0xA0, 0xA0, 0xA0),
                Color.FromArgb(0x50, 0x50, 0x50),
                Color.FromArgb(0x50, 0x50, 0xff),
                Color.FromArgb(0x00, 0xff, 0x50),
                Color.FromArgb(0xff, 0xff, 0x50),
                Color.FromArgb(0xff, 0x50, 0x50),
                Color.FromArgb(0xff, 0x50, 0xff),
                Color.FromArgb(0xff, 0xff, 0x50),
                Color.FromArgb(0xff, 0xff, 0xff)
            }
        };

        public Color[] Colors { get; set; }

        public byte ColStart { get; private set; }
        public byte Format { get; private set; }

        public Image GetImage()
        {
            int realCount = 0;
            for (int k = Colors.Length - 1; k >= 0; k--)
            {
                if (!Colors[k].IsEmpty)
                {
                    realCount = k + 1;
                    break;
                }
            }

            int height = realCount / 16;
            if (realCount % 16 > 0) height++;

            var bmp = new Bitmap(16, height, PixelFormat.Format24bppRgb);

            int ind = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (ind >= Colors.Length) break;

                    bmp.SetPixel(x, y, Colors[ind]);
                    ind++;
                }
                if (ind >= Colors.Length) break;
            }

            return bmp;
        }

        public static Palette Read(MemoryStream ms)
        {
            var pal = new Palette();

            var palOffset = ms.Position;

            var marker = ms.ReadB();
            var garbage = ms.ReadBytes(9);
            ms.Position += 2 + 1;
            //if (ms.ReadUShortBE() != 1) throw new FormatException();
            //if (ms.ReadB() != 0) throw new FormatException();

            var offsetEnd = ms.ReadUShortBE();
            var globalMarker = ms.ReadUIntBE();
            ms.Position += 4 + 2;
            //if (ms.ReadUShortBE() != 0) throw new FormatException();
            pal.ColStart = ms.ReadB();
            ms.Position += 3;
            var colCount = ms.ReadUShortBE();

            ms.Position += 1;
            pal.Format = ms.ReadB();

            if (marker == 0 && garbage[0] == 1)
            {
                pal.Format = 0;
                pal.ColStart = 0;
                colCount = 256;
                palOffset += 260;
            }
            else
            {
                palOffset += 37;
            }

            pal.Colors = new Color[colCount];

            if (colCount > 0)
            {
                ms.Position = palOffset;

                if (pal.ColStart + colCount > 256) throw new FormatException();

                if (pal.Format == 0)
                {
                    for (int i = 0; i < colCount; i++)
                    {
                        var used = ms.ReadB();
                        var r = ms.ReadB();
                        var g = ms.ReadB();
                        var b = ms.ReadB();
                        if (used > 0)
                            pal.Colors[i] = Color.FromArgb(r, g, b);
                    }
                }
                else if (pal.Format == 1)
                {
                    for (int i = 0; i < colCount; i++)
                    {
                        var r = ms.ReadB();
                        var g = ms.ReadB();
                        var b = ms.ReadB();
                        pal.Colors[i] = Color.FromArgb(r, g, b);
                    }
                }
                else throw new FormatException();

                return pal;
            }

            return pal;
        }

        public void Write(ByteBuilder bb)
        {
            bb.AddByte(0xe); // Marker
            bb.Zeros(9);
            bb.AddShortBE(1);
            bb.AddByte(0);
            int offsetEndRef = bb.Position;
            bb.AddShortBE(0); // Offset end
            bb.AddIntBE(0x393939); // Global marker
            bb.Zeros(6);
            bb.AddByte(ColStart);
            bb.Zeros(3);
            bb.AddUShortBE((ushort)Colors.Length);
            bb.AddByte(0);
            bb.AddByte(Format);
            bb.AddIntBE(0);

            for (int i = 0; i < Colors.Length; i++)
            {
                var c = Colors[i];
                if (Format == 0)
                {
                    if (c.IsEmpty)
                    {
                        bb.AddByte(0);
                        bb.AddByte(0);
                        bb.AddByte(0);
                        bb.AddByte(0);
                    }
                    else
                    {
                        bb.AddByte(1);
                        bb.AddByte(c.R);
                        bb.AddByte(c.G);
                        bb.AddByte(c.B);
                    }
                }
                else
                {
                    bb.AddByte(c.R);
                    bb.AddByte(c.G);
                    bb.AddByte(c.B);
                }
            }

            bb.SetUShortBE(offsetEndRef, (ushort)(bb.Position - offsetEndRef)); // Setup offset end
        }

        public byte GetColorIndex(Color color)
        {
            int bestDiff = 0;
            byte best = 0;

            for (int i = 0; i < Colors.Length; i++)
            {
                var c = Colors[i];
                if (c == color) return (byte)i;

                var d = Math.Abs(c.R - color.R) + Math.Abs(c.G - color.G) + Math.Abs(c.B - color.B);
                if (bestDiff == 0 || d < bestDiff)
                {
                    bestDiff = d;
                    best = (byte)i;
                }
            }

            return best;
        }
    }
}
