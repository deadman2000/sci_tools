using SCI_Lib.Utils;
using System.Drawing;
using System.IO;

namespace SCI_Lib.Resources.Picture
{
    class PicPalette : PicExtCommand
    {
        private byte[] mapping;
        private int ts;
        public PalColor[] Colors { get; set; }

        public PicPalette(Stream stream) : base(0x02)
        {
            mapping = new byte[256];
            stream.Read(mapping, 0, mapping.Length);
            ts = stream.ReadIntBE();

            Colors = new PalColor[256];

            for (int i = 0; i < 256; i++)
            {
                var c = new PalColor
                {
                    Used = stream.ReadB(),
                    R = stream.ReadB(),
                    G = stream.ReadB(),
                    B = stream.ReadB()
                };
                Colors[i] = c;

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
    }

    class PalColor
    {
        public byte Used;
        public byte R;
        public byte G;
        public byte B;

        public Color GetColor()
        {
            return Color.FromArgb(R, G, B);
        }
    }
}
