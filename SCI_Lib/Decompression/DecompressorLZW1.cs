using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Decompression
{
    class DecompressorLZW1 : Decompressor
    {
        private readonly Stack<byte> stack = new();
        private readonly LZWToken[] tokens = new LZWToken[0x1004];

        protected override void GoUnpack()
        {
            var reader = new BitReaderMSB(_stream);
            BitReaderMSB.DEBUG = DEBUG;

            ushort numbits = 9;
            ushort curtoken = 0x102;
            ushort endtoken = 0x1ff;

            byte lastchar = 0;
            ushort lastbits = 0;

            bool firstChar = true;
            ushort bitstring;

            while (!IsFinished())
            {
                if (DEBUG) Console.WriteLine();
                if (DEBUG) Console.WriteLine(_dwWrote);

                bitstring = (ushort)reader.GetBits(numbits);

                if (bitstring == 0x101) // found end-of-data signal
                {
                    if (DEBUG) Console.WriteLine("End");
                    return;
                }

                if (bitstring == 0x100) // start-over signal
                {
                    if (DEBUG) Console.WriteLine("Reset");
                    numbits = 9;
                    curtoken = 0x102;
                    endtoken = 0x1ff;
                    firstChar = true;
                    continue;
                }

                if (firstChar)
                {
                    if (DEBUG) Console.WriteLine("First");
                    lastchar = (byte)(bitstring & 0xff);
                    lastbits = bitstring;
                    firstChar = false;

                    PutByte(lastchar);
                }
                else
                {
                    ushort token = bitstring;
                    if (token == curtoken) // index past current point
                    {
                        if (DEBUG) Console.WriteLine($"Push lastchar {lastchar:X02}");
                        token = lastbits;
                        stack.Push(lastchar);
                    }

                    while ((token > 0xff) && (token < 0x1004)) // follow links back in data
                    {
                        if (DEBUG) Console.WriteLine($"Push token[{token:X4}] {tokens[token]}");
                        stack.Push(tokens[token].data);
                        token = tokens[token].next;
                    }
                    lastchar = (byte)(token & 0xff);
                    stack.Push(lastchar);

                    WriteStack();

                    // put token into record
                    if (curtoken <= endtoken)
                    {
                        tokens[curtoken].data = lastchar;
                        tokens[curtoken].next = lastbits;
                        if (DEBUG) Console.WriteLine($"tokens[{curtoken:X4}] = {tokens[curtoken]}");
                        curtoken++;
                        if (curtoken == endtoken && numbits < 12)
                        {
                            numbits++;
                            endtoken = (ushort)((endtoken << 1) + 1);
                            if (DEBUG) Console.WriteLine($"New endtoken = {endtoken:X4} numbits = {numbits}");
                        }
                    }
                    lastbits = bitstring;
                }
            }
        }

        private void WriteStack()
        {
            while (stack.Any())
            {
                PutByte(stack.Pop());

                if (_dwWrote == _szUnpacked)
                    return;
            }
        }
    }

    struct LZWToken
    {
        public byte data;
        public ushort next;
        public override readonly string ToString() => $"{data:X02} => {next:X04}";
    };
}
