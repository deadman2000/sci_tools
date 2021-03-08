using System;
using System.IO;

namespace SCI_Lib.Utils
{
    public class BitWriterMSB
    {
        internal static bool DEBUG = false;

        private byte _buff;
        private int _bitsCount;

        private readonly Stream _stream;

        public BitWriterMSB(Stream stream)
        {
            _stream = stream;
        }

        public void WriteBits(int value, int count)
        {
            if (DEBUG) Console.WriteLine($"{count} < {value:X2}   pos:{_stream.Position}");
            while (count > 0)
            {
                if (count + _bitsCount >= 8)
                {
                    _buff |= (byte)(value >> (count - 8 + _bitsCount));
                    _stream.WriteByte(_buff);

                    count -= 8 - _bitsCount;
                    _bitsCount = 0;
                    _buff = 0;
                }
                else
                {
                    value &= (0xff >> (8 - count));
                    _buff |= (byte)(value << (8 - count - _bitsCount));
                    _bitsCount += count;
                    break;
                }
            }
        }

        public void Flush()
        {
            if (_bitsCount > 0)
            {
                if (DEBUG) Console.WriteLine($"Flush {_bitsCount}  [{_buff:X2}]");
                _stream.WriteByte(_buff);
            }
        }
    }
}
