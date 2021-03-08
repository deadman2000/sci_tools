using System;
using System.IO;

namespace SCI_Lib.Decompression
{
    public abstract class Decompressor
    {
        public static bool DEBUG = false;

        protected int _szPacked;	// size of the compressed data
        protected int _szUnpacked;	// size of the decompressed data
        protected int _dwWrote;     // number of bytes written to _dest
        protected Stream _stream;
        protected byte[] _dest;

        private long _initPos;

        public byte[] Unpack(Stream stream, int nPacked, int nUnpacked)
        {
            _stream = stream;
            _szPacked = nPacked;
            _szUnpacked = nUnpacked;
            _dwWrote = 0;

            _initPos = _stream.Position;

            _dest = new byte[nUnpacked];
            GoUnpack();

            return _dest;
        }

        protected int BytesRead => (int)(_stream.Position - _initPos);

        protected abstract void GoUnpack();

        protected bool IsFinished()
        {
            return (_dwWrote == _szUnpacked) && (BytesRead >= _szPacked);
        }

        public byte ReadByte()
        {
            int b = _stream.ReadByte();
            if (b < 0) throw new IOException();
            return (byte)b;
        }

        protected byte[] Read(int count)
        {
            byte[] buff = new byte[count];
            int c = _stream.Read(buff, 0, count);
            if (c != count) throw new IOException();
            return buff;
        }

        protected void PutByte(byte b)
        {
            if (DEBUG) Console.WriteLine($"< {b:X02}");
            _dest[_dwWrote++] = b;
        }
    }
}
