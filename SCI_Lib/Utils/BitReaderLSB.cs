using System.IO;

namespace SCI_Lib.Utils
{
    public class BitReaderLSB
    {
        private uint _dwBits;       // bits buffer
        private byte _nBits;        // number of unread bits in _dwBits
        private readonly Stream _stream;

        public BitReaderLSB(Stream stream)
        {
            _stream = stream;
        }

        public uint GetBits(int n)
        {
            while (_nBits < n)
            {
                _dwBits |= ((uint)_stream.ReadByte()) << _nBits;
                _nBits += 8;
            }

            uint ret = (uint)(_dwBits & ~((~0) << n));
            _dwBits >>= n;
            _nBits = (byte)(_nBits - n);
            return ret;
        }

        public byte GetByte()
        {
            return (byte)GetBits(8);
        }
    }
}
