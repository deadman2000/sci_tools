using System;
using System.IO;

namespace SCI_Lib.Utils
{
    public class BitWriterLSB
    {
        private byte _buff;
        private int _bitsCount;

        private readonly Stream _stream;

        public BitWriterLSB(Stream stream)
        {
            _stream = stream;
        }


        public void WriteBits(int value, int count)
        {
            if (_bitsCount > 0)
            {
                _buff |= (byte)(value << _bitsCount);
                if (_bitsCount + count > 8)
                {
                    _stream.WriteByte(_buff);

                    int c = 8 - _bitsCount;
                    value >>= c;
                    count -= c;

                    _bitsCount = 0;
                    _buff = 0;
                }
                else
                {
                    _bitsCount += count;
                    return;
                }
            }

            while (count >= 8)
            {
                _stream.WriteByte((byte)(value & 0xff));
                value >>= 8;
                count -= 8;
            }

            if (count == 0)
            {
                if (_bitsCount != 0 || _buff != 0) throw new Exception();
                return;
            }

            _buff = (byte)value;
            _bitsCount = count;
        }

        public void Flush()
        {
            if (_bitsCount > 0)
            {
                _stream.WriteByte(_buff);
            }
        }
    }
}
