using SCI_Lib.Pictures;
using SCI_Lib.Utils;
using System;
using System.IO;

namespace SCI_Lib.Resources
{
    public class ResFont : Resource
    {
        private SCIFont _font;

        public SCIFont GetFont()
        {
            if (_font != null) return _font;

            byte[] data = GetContent();
            if (data == null) return null;

            using var stream = new MemoryStream(data.Length);
            stream.Write(data, 0, data.Length);
            stream.Seek(0, SeekOrigin.Begin);

            var zero = stream.ReadUShortBE();
            if (zero != 0) throw new Exception("Wrong font format");

            var charCount = stream.ReadUShortBE();
            var spr = new SCIFont();
            spr.FontHeight = stream.ReadUShortBE();

            ushort[] offsets = new ushort[charCount];
            for (int i = 0; i < charCount; i++)
                offsets[i] = stream.ReadUShortBE();

            byte bit;
            byte c;

            for (int i = 0; i < charCount; i++)
            {
                stream.Seek(offsets[i], SeekOrigin.Begin);

                byte w = stream.ReadB();
                byte h = stream.ReadB();

                SpriteFrame frm = new SpriteFrame(w, h);
                var val = stream.ReadByte();
                for (int y = 0; y < h; y++)
                {
                    bit = 0;

                    for (byte x = 0; x < w; x++)
                    {
                        if ((val & (1 << (7 - bit))) > 0)
                            c = 1;
                        else
                            c = 0;

                        frm[x, y] = c;

                        bit++;
                        if (bit == 8)
                        {
                            bit = 0;
                            val = stream.ReadByte();
                        }
                    }

                    if (bit != 0)
                        val = stream.ReadByte();
                }

                spr.Frames.Add(frm);
            }

            return _font = spr;
        }

        public void SetFont(SCIFont spr)
        {
            _font = spr;
        }

        public override byte[] GetPatch()
        {
            if (_font == null) return GetContent();

            ushort cnt = (ushort)(_font.Frames.Count & 0xFFFF);

            ByteBuilder bb = new ByteBuilder();
            bb.AddShortBE(0);
            bb.AddUShortBE(cnt);
            bb.AddUShortBE(_font.FontHeight);

            for (int i = 0; i < cnt; i++)
                bb.AddShortBE(0);

            byte bit;
            byte bitMask;
            for (int i = 0; i < cnt; i++)
            {

                SpriteFrame frm = _font[i];
                byte w = (byte)frm.Width;
                byte h = (byte)frm.Height;
                ushort offset = (ushort)bb.Position;

                bb.SetUShortBE(6 + i * 2, offset);

                bb.AddByte(w);
                bb.AddByte(h);
                for (int y = 0; y < frm.Height; y++)
                {
                    bit = 0;
                    bitMask = 0;
                    for (int x = 0; x < frm.Width; x++)
                    {
                        if (frm[x, y] == 1)
                            bitMask |= (byte)(1 << (7 - bit));

                        bit++;
                        if (bit == 8)
                        {
                            bit = 0;
                            bb.AddByte(bitMask);
                            bitMask = 0;
                        }
                    }

                    if (bit != 0)
                        bb.AddByte(bitMask);
                }
            }

            return bb.GetArray();
        }

        public void GenerateOutline(ResFont sourceResFont, int startChar, int endChar = 0)
        {
            var dst = GetFont();
            var src = sourceResFont.GetFont();
            if (endChar == 0)
                endChar = src.Frames.Count - 1;

            while (dst.Frames.Count < endChar + 1)
                dst.Frames.Add(null);

            for (int i = startChar; i <= endChar; i++)
                dst.Frames[i] = src[i].GetOutline();
        }
    }
}
