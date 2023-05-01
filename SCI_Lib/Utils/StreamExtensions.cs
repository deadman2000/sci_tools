using System;
using System.Collections.Generic;
using System.IO;

namespace SCI_Lib.Utils
{
    public static class StreamExtensions
    {
        public static ushort ReadUShortBE(this Stream stream)
        {
            return (ushort)(stream.ReadByte() | (stream.ReadByte() << 8));
        }

        public static short ReadShortBE(this Stream stream)
        {
            return (short)(stream.ReadByte() | (stream.ReadByte() << 8));
        }

        public static void WriteUShortBE(this Stream stream, ushort val)
        {
            stream.WriteByte((byte)val);
            stream.WriteByte((byte)(val >> 8));
        }

        public static ushort ReadUShortLE(this Stream stream)
        {
            return (ushort)((stream.ReadByte() << 8) | stream.ReadByte());
        }

        public static void WriteUShortLE(this Stream stream, ushort val)
        {
            stream.WriteByte((byte)(val >> 8));
            stream.WriteByte((byte)val);
        }

        public static int Read3ByteBE(this Stream stream)
        {
            return stream.ReadByte() | (stream.ReadByte() << 8) | (stream.ReadByte() << 16);
        }

        public static int ReadIntBE(this Stream stream)
        {
            return stream.ReadByte() | (stream.ReadByte() << 8) | (stream.ReadByte() << 16) | (stream.ReadByte() << 24);
        }

        public static void WriteIntBE(this Stream stream, int val)
        {
            stream.WriteByte((byte)val);
            stream.WriteByte((byte)(val >> 8));
            stream.WriteByte((byte)(val >> 16));
            stream.WriteByte((byte)(val >> 24));
        }

        public static uint ReadUIntBE(this Stream stream)
        {
            return (uint)(stream.ReadByte() | (stream.ReadByte() << 8) | (stream.ReadByte() << 16) | (stream.ReadByte() << 24));
        }

        public static void WriteUIntBE(this Stream stream, uint val)
        {
            stream.WriteByte((byte)val);
            stream.WriteByte((byte)(val >> 8));
            stream.WriteByte((byte)(val >> 16));
            stream.WriteByte((byte)(val >> 24));
        }

        public static void Write(this Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }

        public static uint ReadUIntLE(this Stream stream)
        {
            return (uint)((stream.ReadByte() << 24) | (stream.ReadByte() << 16) | (stream.ReadByte() << 8) | stream.ReadByte());
        }

        public static string ReadString(this Stream stream, GameEncoding encoding)
        {
            List<byte> buff = new List<byte>();
            while (true)
            {
                var b = stream.ReadByte();
                if (b == 0) break;
                buff.Add((byte)b);
            }
            return encoding.GetString(buff.ToArray());
        }

        public static byte[] ReadBytes(this Stream stream, int length)
        {
            byte[] bytes = new byte[length];
            if (stream.Read(bytes, 0, length) != length)
                throw new FormatException();

            return bytes;
        }

        public static byte ReadB(this Stream stream)
        {
            var b = stream.ReadByte();
            if (b < 0 || b > 255) throw new FormatException();
            return (byte)b;
        }

        public static byte Peek(this Stream stream)
        {
            var pos = stream.Position;
            var b = stream.ReadB();
            stream.Seek(pos, SeekOrigin.Begin);
            return b;
        }

        public static PointShort ReadPicAbsCoord(this Stream stream)
        {
            var prefix = stream.ReadB();
            PointShort p;
            p.X = (ushort)(stream.ReadB() + ((prefix & 0xf0) << 4));
            p.Y = (ushort)(stream.ReadB() + ((prefix & 0x0f) << 8));
            return p;
        }

        public static PointShort ReadPicRelCoord(this Stream stream, PointShort orig)
        {
            var i = stream.ReadB();
            if ((i & 0x80) == 0x80)
                orig.X = (ushort)(orig.X - ((i >> 4) & 0xf));
            else
                orig.X = (ushort)(orig.X + ((i >> 4) & 0xf));

            if ((i & 0x08) == 0x08)
                orig.Y = (ushort)(orig.Y - (i & 7));
            else
                orig.Y = (ushort)(orig.Y + (i & 7));
            return orig;
        }

        public static PointByte ReadPointByte(this Stream stream)
        {
            PointByte p;
            p.X = stream.ReadB();
            p.Y = stream.ReadB();
            return p;
        }
    }

    public struct PointShort
    {
        public ushort X;
        public ushort Y;
    }

    public struct PointByte
    {
        public byte X;
        public byte Y;
    }
}
