using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCI_Lib
{
    public class GameEncoding
    {
        public GameEncoding(Encoding encoding)
        {
            Encoding = encoding;

            byte[] bytes = new byte[256];
            for (int i = 0; i < 256; i++)
                bytes[i] = (byte)i;
            AllChars = Encoding.GetChars(bytes);
        }

        public Encoding Encoding { get; private set; }

        public char[] AllChars { get; private set; }

        public string GetString(byte[] data)
        {
            char[] chars = Encoding.GetChars(data);
            return new string(chars);
        }

        public string GetString(byte[] data, int from, int length)
        {
            char[] chars = Encoding.GetChars(data, from, length);
            return new string(chars);
        }

        public string ByteToHexTable(byte[] data)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            while (i < data.Length)
            {
                sb.Append(String.Format("{0:X8}: {1,-24} {2,-24}  {3}\r\n", i, Helpers.ByteToHex(data, i, 8), Helpers.ByteToHex(data, i + 8, 8), PrintableString(data, i, 16)));
                i += 16;
            }
            return sb.ToString().TrimEnd();
        }

        static char CharToPrint(char c, char def)
        {
            if (!Char.IsControl(c))
                return c;
            else
                return def;
        }

        public string PrintableString(byte[] data, int offset, int len)
        {
            StringBuilder sb = new StringBuilder();
            char[] str866;
            if (data.Length > offset + len)
                str866 = Encoding.GetChars(data, offset, len);
            else
                str866 = Encoding.GetChars(data, offset, data.Length - offset);

            for (int i = offset; i < offset + len && i < data.Length; i++)
            {
                sb.Append(CharToPrint(str866[i - offset], '.'));
            }
            return sb.ToString().TrimEnd();
        }

        private static Escaper escaper = new DollarEscaper();

        public static byte[] Unescape(byte[] data) => escaper.Unescape(data);

        public byte[] GetBytes(string text) => Encoding.GetBytes(text);

        public byte[] GetBytesUnescape(string text) => escaper.Unescape(Encoding.GetBytes(text));

        public string GetStringEscape(byte[] data)
        {
            char[] str866 = Encoding.GetChars(data);
            return escaper.Escape(str866);
        }

        public string GetStringEscape(byte[] data, int from, int length)
        {
            char[] str866 = Encoding.GetChars(data, from, length);
            return escaper.Escape(str866);
        }

        abstract class Escaper
        {
            public abstract string Escape(char[] str866);
            public abstract byte[] Unescape(byte[] str866);
        }

        class DollarEscaper : Escaper
        {
            public override string Escape(char[] str866)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < str866.Length; i++)
                {
                    if (Char.IsControl(str866[i]))
                    {
                        sb.AppendFormat("${0:X2}", (byte)str866[i]);
                    }
                    else if (str866[i] == '$')
                        sb.Append("$$");
                    else
                        sb.Append(str866[i]);
                }
                return sb.ToString();
            }

            public override byte[] Unescape(byte[] str866)
            {
                List<byte> bb = new List<byte>();
                for (int c = 0; c < str866.Length; c++)
                {
                    if (str866[c] == 0) continue;

                    if (str866[c] == '$')
                    {
                        if (str866[c + 1] == '$')
                        {
                            bb.Add((byte)'$');
                            c++;
                        }
                        else
                        {
                            string hex = String.Concat((char)str866[c + 1], (char)str866[c + 2]);
                            bb.Add(Convert.ToByte(hex, 16));
                            c += 2;
                        }
                    }
                    else
                    {
                        bb.Add(str866[c]);
                    }
                }

                return bb.ToArray();
            }

            private static char GetEscape(byte val)
            {
                switch (val)
                {
                    case 9: return 't';
                    case 0xa: return 'n';
                    case 0xd: return 'r';
                    default: throw new NotImplementedException();
                }
            }

            private static byte GetUnescape(char val)
            {
                switch (val)
                {
                    case 't': return 9;
                    case 'n': return 0xa;
                    case 'r': return 0xd;
                    default: throw new NotImplementedException();
                }
            }
        }

        class SlashEscaper : Escaper
        {
            public override string Escape(char[] str866)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < str866.Length; i++)
                {
                    if (Char.IsControl(str866[i]))
                    {
                        sb.Append('\\').Append(GetEscape((byte)str866[i]));
                    }
                    else if (str866[i] == '\\')
                        sb.Append("\\\\");
                    else
                        sb.Append(str866[i]);
                }
                return sb.ToString();
            }

            public override byte[] Unescape(byte[] str866)
            {
                List<byte> bb = new List<byte>();
                for (int c = 0; c < str866.Length; c++)
                {
                    if (str866[c] == 0) continue;

                    if (str866[c] == '\\')
                    {
                        if (str866[c + 1] == '\\')
                        {
                            bb.Add((byte)'\\');
                            c++;
                        }
                        else
                        {
                            bb.Add(GetUnescape((char)str866[c + 1]));
                            c += 1;
                        }
                    }
                    else
                    {
                        bb.Add(str866[c]);
                    }
                }

                return bb.ToArray();
            }

            private static char GetEscape(byte val)
            {
                switch (val)
                {
                    case 9: return 't';
                    case 0xa: return 'n';
                    case 0xd: return 'r';
                    default: throw new NotImplementedException();
                }
            }

            private static byte GetUnescape(char val)
            {
                switch (val)
                {
                    case 't': return 9;
                    case 'n': return 0xa;
                    case 'r': return 0xd;
                    default: throw new NotImplementedException();
                }
            }
        }
    }
}
