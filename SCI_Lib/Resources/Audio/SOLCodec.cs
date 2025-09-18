using SCI_Lib.Utils;
using System;
using System.IO;

namespace SCI_Lib.Resources.Audio
{
    public static class SOLCodec
    {
        public static byte[] Decode(Stream stream, int size)
        {
            int s = 0;
            MemoryStream ms = new(size * 2);
            for (int i = 0; i < size; i++)
            {
                var b = stream.ReadB();
                if ((b & 0x80) != 0)
                    s -= SOLTable[b & 0x7f];
                else
                    s += SOLTable[b];

                short value = (short)Clip(s, -32768, 32767);
                ms.WriteShortBE(value);
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray();
        }

        public static byte[] Encode(Stream stream)
        {
            MemoryStream ms = new();
            byte[] block = new byte[2];

            int prev = 0;
            while (true)
            {
                if (stream.Read(block) != 2) break;

                var value = BitConverter.ToInt16(block);
                var diff = prev - value;
                var index = GetSOLTableIndex((ushort)Math.Abs(diff));
                if (diff < 0) index |= 0x80;

                ms.WriteByte(index);

                prev = value;
            }

            return ms.ToArray();
        }

        private static int Clip(int value, int min, int max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        private static readonly ushort[] SOLTable = new ushort[] {
            0x0,   0x8,    0x10,   0x20,   0x30,   0x40,   0x50,   0x60,
            0x70,  0x80,   0x90,   0xA0,   0xB0,   0xC0,   0xD0,   0xE0,
            0xF0,  0x100,  0x110,  0x120,  0x130,  0x140,  0x150,  0x160,
            0x170, 0x180,  0x190,  0x1A0,  0x1B0,  0x1C0,  0x1D0,  0x1E0,
            0x1F0, 0x200,  0x208,  0x210,  0x218,  0x220,  0x228,  0x230,
            0x238, 0x240,  0x248,  0x250,  0x258,  0x260,  0x268,  0x270,
            0x278, 0x280,  0x288,  0x290,  0x298,  0x2A0,  0x2A8,  0x2B0,
            0x2B8, 0x2C0,  0x2C8,  0x2D0,  0x2D8,  0x2E0,  0x2E8,  0x2F0,
            0x2F8, 0x300,  0x308,  0x310,  0x318,  0x320,  0x328,  0x330,
            0x338, 0x340,  0x348,  0x350,  0x358,  0x360,  0x368,  0x370,
            0x378, 0x380,  0x388,  0x390,  0x398,  0x3A0,  0x3A8,  0x3B0,
            0x3B8, 0x3C0,  0x3C8,  0x3D0,  0x3D8,  0x3E0,  0x3E8,  0x3F0,
            0x3F8, 0x400,  0x440,  0x480,  0x4C0,  0x500,  0x540,  0x580,
            0x5C0, 0x600,  0x640,  0x680,  0x6C0,  0x700,  0x740,  0x780,
            0x7C0, 0x800,  0x900,  0xA00,  0xB00,  0xC00,  0xD00,  0xE00,
            0xF00, 0x1000, 0x1400, 0x1800, 0x1C00, 0x2000, 0x3000, 0x4000
        };

        private static byte GetSOLTableIndex(ushort value)
        {
            int i0 = 0;
            int i2 = SOLTable.Length - 1;

            if (SOLTable[i0] == value) return (byte)i0;
            if (SOLTable[i2] == value) return (byte)i2;

            while (true)
            {
                if (i2 - i0 == 1)
                {
                    var v0 = SOLTable[i0];
                    var v2 = SOLTable[i2];
                    if (value - v0 < v2 - value) return (byte)i0;

                    return (byte)i2;
                }

                int i1 = (i0 + i2) / 2;
                var v1 = SOLTable[i1];

                if (v1 == value) return (byte)i1;

                if (v1 < value)
                    i0 = i1;
                else
                    i2 = i1;
            }
        }
    }
}
