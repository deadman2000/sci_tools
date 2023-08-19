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

        public char[] GetChars(byte[] data) => Encoding.GetChars(data);

        public string GetString(byte[] data)
        {
            char[] chars = Encoding.GetChars(data);
            return new string(chars);
        }

        public string GetString(byte[] data, int from, int length)
        {
            if (length == 0) return string.Empty;
            char[] chars = Encoding.GetChars(data, from, length);
            return new string(chars);
        }

        public string GetString(byte[] data, int from)
        {
            for (int i = from; i < data.Length; i++)
            {
                if (data[i] == 0)
                    return GetString(data, from, i - from);
            }
            return GetString(data, from, data.Length - from);
        }

        public string ByteToHexTable(byte[] data)
        {
            int i = 0;
            StringBuilder sb = new();
            while (i < data.Length)
            {
                sb.Append(string.Format("{0:X8}: {1,-24} {2,-24}  {3}\r\n", i, Helpers.ByteToHex(data, i, 8), Helpers.ByteToHex(data, i + 8, 8), PrintableString(data, i, 16)));
                i += 16;
            }
            return sb.ToString().TrimEnd();
        }

        static char CharToPrint(char c, char def)
        {
            if (!char.IsControl(c))
                return c;
            else
                return def;
        }

        public string PrintableString(byte[] data, int offset, int len)
        {
            StringBuilder sb = new();
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

        public BaseEscaper Escaper { get; set; } = BaseEscaper.Dollar;

        public byte[] Unescape(byte[] data) => Escaper.Unescape(data);

        public byte[] GetBytes(string text) => Encoding.GetBytes(text);

        public byte[] GetBytesUnescape(string text) => Escaper.Unescape(Encoding.GetBytes(text));

        public string GetStringEscape(byte[] data) => Escaper.Escape(Encoding.GetChars(data));

        public string GetStringEscape(byte[] data, int from, int length) => Escaper.Escape(Encoding.GetChars(data, from, length));

        public string EscapeString(string str) => Escaper.Escape(str);

        public string UnescapeString(string str) => GetString(GetBytesUnescape(str));
    }
}
