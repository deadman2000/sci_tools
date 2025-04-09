using System;
using System.Collections.Generic;

namespace SCI_Lib.Utils
{
    public class ByteBuilder
    {
        private List<byte> _bytes = new List<byte>();

        public int Position => _bytes.Count;


        public byte[] GetArray()
        {
            return _bytes.ToArray();
        }


        public void AddBytes(IEnumerable<byte> bytes)
        {
            _bytes.AddRange(bytes);
        }

        public void AddByte(byte b)
        {
            _bytes.Add(b);
        }

        public void AddByte(char c)
        {
            _bytes.Add((byte)c);
        }

        public void AddUShortLE(ushort val)
        {
            _bytes.Add((byte)(val >> 8));
            _bytes.Add((byte)(val & 0xFF));
        }

        public void AddUShortBE(ushort val)
        {
            _bytes.Add((byte)(val & 0xFF));
            _bytes.Add((byte)(val >> 8));
        }

        public void AddShortLE(short val)
        {
            _bytes.Add((byte)(val >> 8));
            _bytes.Add((byte)(val & 0xFF));
        }

        public void AddShortBE(short val)
        {
            _bytes.Add((byte)(val & 0xFF));
            _bytes.Add((byte)(val >> 8));
        }

        public void AddIntBE(int val)
        {
            _bytes.Add((byte)(val & 0xFF));
            _bytes.Add((byte)(val >> 8));
            _bytes.Add((byte)(val >> 16));
            _bytes.Add((byte)(val >> 24));
        }

        public void AddIntBE(uint val)
        {
            _bytes.Add((byte)(val & 0xFF));
            _bytes.Add((byte)(val >> 8));
            _bytes.Add((byte)(val >> 16));
            _bytes.Add((byte)(val >> 24));
        }

        public void AddThreeBytesBE(int val)
        {
            _bytes.Add((byte)(val & 0xFF));
            _bytes.Add((byte)(val >> 8));
            _bytes.Add((byte)(val >> 16));
        }

        public void AddThreeBytesLE(int val)
        {
            _bytes.Add((byte)(val >> 16));
            _bytes.Add((byte)(val >> 8));
            _bytes.Add((byte)(val & 0xFF));
        }

        public void SetByte(int offset, byte val)
        {
            _bytes[offset] = val;
        }

        public void SetSByte(int offset, sbyte val)
        {
            _bytes[offset] = (byte)val;
        }

        public void SetUShortBE(int offset, ushort val)
        {
            _bytes[offset] = (byte)(val & 0xFF);
            _bytes[offset + 1] = (byte)(val >> 8);
        }

        public void SetShortBE(int offset, short val)
        {
            _bytes[offset] = (byte)(val & 0xFF);
            _bytes[offset + 1] = (byte)(val >> 8);
        }

        public void SetIntBE(int offset, int val)
        {
            _bytes[offset] = (byte)(val & 0xFF);
            _bytes[offset + 1] = (byte)(val >> 8);
            _bytes[offset + 2] = (byte)(val >> 16);
            _bytes[offset + 3] = (byte)(val >> 24);
        }


        public void WritePicAbsCoord(ref PointShort p)
        {
            /*  var prefix = stream.ReadB();
                p.X = (ushort)(stream.ReadB() + ((prefix & 0xf0) << 4));
                p.Y = (ushort)(stream.ReadB() + ((prefix & 0x0f) << 8)); */
            byte prefix = (byte)((((p.X >> 8) & 0xf) << 4) | ((p.Y >> 8) & 0xf));
            AddByte(prefix);
            AddByte((byte)(p.X & 0xff));
            AddByte((byte)(p.Y & 0xff));
        }

        public void Zeros(int count)
        {
            for (int i = 0; i < count; i++) AddByte(0);
        }

        public void AddString(string value, GameEncoding encoding)
        {
            _bytes.AddRange(encoding.GetBytes(value));
        }
    }
}
