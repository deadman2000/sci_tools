using System;
using System.IO;

namespace SCI_Lib.Utils
{
    public class BitReaderMSB
    {
        private uint _dwBits;       // bits buffer
        private byte _nBits;        // number of unread bits in _dwBits
        private readonly Stream _stream;
        private readonly long _startPos;

        public BitReaderMSB(Stream stream)
        {
            _stream = stream;
            _startPos = stream.Position;
        }

        internal static bool DEBUG = false;

        public int Position => (int)(_stream.Position - _startPos);

        public uint GetBits(ushort n)
        {
            while (_nBits < n)
            {
                _dwBits |= ((uint)_stream.ReadByte()) << (24 - _nBits);
                _nBits += 8;
            }

            uint ret = _dwBits >> (32 - n);
            _dwBits <<= n;
            _nBits = (byte)(_nBits - n);
            if (DEBUG) Console.WriteLine($"{n} > {ret:X02}   pos:{_stream.Position - _startPos}");
            return ret;
        }

        public byte GetByte()
        {
            return (byte)GetBits(8);
        }
    }
}
