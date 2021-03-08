using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SCI_Lib.Utils;

namespace SCI_Lib.Decompression
{
    class DecompressorHuffman : Decompressor
    {
        protected byte[] _nodes;
        private BitReaderMSB reader;

        protected override void GoUnpack()
        {
            reader = new BitReaderMSB(_stream);

            byte numnodes;
            short c;
            ushort terminator;

            numnodes = ReadByte();
            terminator = (ushort)(ReadByte() | 0x100);
            _nodes = Read(numnodes << 1);

            while ((c = Getc2()) != terminator && (c >= 0) && !IsFinished())
                PutByte((byte)c);

            _nodes = null;
        }

        protected short Getc2()
        {
            int i = 0;
            short next;
            while (_nodes[i + 1] > 0)
            {
                if (reader.GetBits(1) > 0)
                {
                    next = (short)(_nodes[i + 1] & 0x0F); // use lower 4 bits
                    if (next == 0)
                        return (short)(reader.GetByte() | 0x100);
                }
                else
                    next = (short)(_nodes[i + 1] >> 4); // use higher 4 bits
                i += next << 1;
            }
            return (short)(_nodes[i] | (_nodes[i + 1] << 8));
        }

    }
}
