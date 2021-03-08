using SCI_Lib.Decompression;
using SCI_Lib.Utils;
using System;
using System.Collections.Generic;

namespace SCI_Lib.Compression
{
    class CompressorLZW1 : Compressor
    {
        BitWriterMSB writer;
        ushort numbits = 9;
        ushort lastBits = 0;
        byte lastVal = 0;

        LZWToken[] tokens = new LZWToken[0x1004];
        int pos = 0;
        ushort curtoken = 0x102;
        ushort endtoken = 0x1ff;
        bool isFirst = true;

        protected override void GoPack()
        {
            writer = new BitWriterMSB(_stream);
            BitWriterMSB.DEBUG = DEBUG;

            while (pos < _data.Length)
            {
                if (curtoken == endtoken && numbits < 12)
                {
                    numbits++;
                    endtoken = (ushort)((endtoken << 1) + 1);
                    if (DEBUG) Console.WriteLine($"New endtoken = {endtoken:X4} numbits = {numbits}");
                }

                if (DEBUG) Console.WriteLine();
                if (DEBUG) Console.WriteLine($"{pos}  {_data[pos]:X2}");

                var val = ReadByte();

                if (isFirst)
                {
                    WriteBits(val);
                    lastVal = val;
                    isFirst = false;
                    continue;
                }

                var bestToken = FindToken(pos - 1);

                if (curtoken < endtoken && lastVal == val && IsToken(lastBits))
                {
                    var data = GetTokenStack(lastBits);
                    if (IsNextBytes(data))
                    {
                        if (bestToken == null || data.Length > bestToken.len)
                        {
                            if (DEBUG) Console.WriteLine($"Repeat last chain. Skip {data.Length - 1}");
                            pos += data.Length - 1;

                            var token = curtoken;
                            AddToken(val, lastBits);
                            WriteBits(token);
                            continue;
                        }
                    }
                }

                if (bestToken != null)
                {
                    if (DEBUG && bestToken != null) Console.WriteLine($"Found token {bestToken.token:X3} skip {bestToken.len}");
                    pos += bestToken.len;
                    AddToken(val, lastBits);
                    WriteBits(bestToken.token);
                }
                else
                {
                    if (curtoken >= endtoken)
                    {
                        Reset();
                    }

                    if (curtoken < endtoken && val == lastBits && pos < _data.Length && _data[pos] == val)
                    {
                        if (DEBUG) Console.WriteLine($"Repeat last char. Skip 1");
                        var token = curtoken;
                        AddToken(val, lastBits);
                        WriteBits(token);
                        pos++;
                    }
                    else
                    {
                        AddToken(val, lastBits);
                        WriteBits(val);
                    }
                }

                lastVal = val;
            }
            WriteBits(0x101);
            writer.Flush();
        }

        void WriteBits(ushort val)
        {
            if (val == 0x1000) throw new InvalidOperationException();
            writer.WriteBits(val, numbits);
            lastBits = val;
        }

        private void Reset()
        {
            if (DEBUG) Console.WriteLine("Reset");
            WriteBits(0x100);
            numbits = 9;
            curtoken = 0x102;
            endtoken = 0x1ff;
            isFirst = true;
        }

        private bool IsNextBytes(byte[] data)
        {
            if (pos + data.Length >= _data.Length) return false;

            for (int i = 0; i < data.Length; i++)
            {
                if (_data[pos + i - 1] != data[i])
                    return false;
            }

            return true;
        }

        private byte[] GetTokenStack(ushort token)
        {
            List<byte> data = new List<byte>();
            data.Add(lastVal);

            while (IsToken(token))
            {
                data.Add(tokens[token].data);
                token = tokens[token].next;
            }
            data.Add((byte)token);
            data.Reverse();
            return data.ToArray();
        }

        class FindResult
        {
            public ushort token;
            public int len;
        }

        private FindResult FindToken(int p)
        {
            if (p >= _data.Length - 1) return null;

            var curr = _data[p];
            var next = _data[p + 1];
            FindResult best = null;

            for (ushort t = 0x102; t < curtoken; t++)
            {
                //if (pos == 454 && t == 0x263) Debugger.Break();

                if (tokens[t].next == curr && tokens[t].data == next)
                {
                    var r = FindToken(p + 2, t);
                    if (r != null && (best == null || best.len <= r.len))
                    {
                        best = new FindResult { token = r.token, len = r.len + 1 };
                    }
                    else if (best == null)
                    {
                        best = new FindResult { token = t, len = 1 };
                    }
                }
            }

            return best;
        }

        private FindResult FindToken(int p, ushort next)
        {
            if (p >= _data.Length - 1) return null;

            var curr = _data[p];
            FindResult best = null;

            for (ushort t = 0x102; t < curtoken; t++)
            {
                if (tokens[t].next == next && tokens[t].data == curr)
                {
                    var r = FindToken(p + 1, t);
                    if (r != null && (best == null || best.len <= r.len))
                    {
                        best = new FindResult { token = r.token, len = r.len + 1 };
                    }
                    else if (best == null)
                    {
                        best = new FindResult { token = t, len = 1 };
                    }
                }
            }

            return best;
        }

        private bool IsToken(ushort val)
        {
            return (val > 0xff) && (val < 0x1004);
        }

        void AddToken(byte data, ushort next)
        {
            if (next == curtoken) throw new InvalidOperationException();
            if (curtoken > endtoken) return;

            tokens[curtoken].data = data;
            tokens[curtoken].next = next;
            if (DEBUG) Console.WriteLine($"tokens[{curtoken:X4}] = {tokens[curtoken]}");
            curtoken++;
        }

        byte ReadByte()
        {
            return _data[pos++];
        }
    }
}
