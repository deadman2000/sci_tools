using SCI_Lib.Utils;
using System;
using System.Drawing;
using System.IO;

namespace SCI_Lib.Resources.Picture
{
    public class PicPalette : PicExtCommand
    {
        private byte[] mapping;
        private int ts;
        public PalColor[] Colors { get; set; }

        private PicPalette() : base(0x02)
        {
        }

        public PicPalette(Stream stream) : base(0x02)
        {
            mapping = new byte[256];
            stream.Read(mapping, 0, mapping.Length);
            ts = stream.ReadIntBE();

            Colors = new PalColor[256];

            for (int i = 0; i < 256; i++)
            {
                var used = stream.ReadB();
                var r = stream.ReadB();
                var g = stream.ReadB();
                var b = stream.ReadB();
                Colors[i] = new PalColor(used, r, g, b);

                // Console.WriteLine($"{i}: {c.Used} [ {c.R:X2} {c.G:X2} {c.B:X2} ]");
            }
        }

        protected override void WriteExt(ByteBuilder bb)
        {
            bb.AddBytes(mapping);
            bb.AddIntBE(ts);

            foreach (var c in Colors)
            {
                bb.AddByte(c.Used);
                bb.AddByte(c.R);
                bb.AddByte(c.G);
                bb.AddByte(c.B);
            }
        }

        public PicPalette ExcludeColors(int[] indexes)
        {
            var colors = new PalColor[Colors.Length];
            Array.Copy(Colors, colors, Colors.Length);

            foreach (var ind in indexes)
            {
                colors[ind] = colors[ind] with
                {
                    Used = 0
                };
            }

            return new PicPalette
            {
                Colors = colors
            };
        }

        public byte GetColorIndex(Color color)
        {
            int bestDiff = 0;
            byte best = 0;

            for (int i = 0; i < Colors.Length; i++)
            {
                if (Colors[i].Used == 0) continue;

                var c = Colors[i].GetColor();
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

    public record struct PalColor(byte Used, byte R, byte G, byte B)
    {
        public Color GetColor()
        {
            return Color.FromArgb(R, G, B);
        }
    }
}
