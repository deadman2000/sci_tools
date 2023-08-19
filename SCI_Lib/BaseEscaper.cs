using System;
using System.Collections.Generic;
using System.Text;

namespace SCI_Lib
{
    public abstract class BaseEscaper

    {
        public static readonly DollarEscaper Dollar = new();
        public static readonly DollarEscaper DollarFull = new(true);
        public static readonly SlashEscaper Slash = new();

        public abstract string Escape(char[] chars);
        public string Escape(string str) => Escape(str.ToCharArray());

        public abstract byte[] Unescape(byte[] bytes);
        public string Unescape(string str, Encoding enc) => enc.GetString(Unescape(enc.GetBytes(str)));
    }

    public class DollarEscaper : BaseEscaper
    {
        private readonly bool _full;

        internal DollarEscaper(bool full = false)
        {
            _full = full;
        }

        public override string Escape(char[] chars)
        {
            StringBuilder sb = new();

            for (int i = 0; i < chars.Length; i++)
            {
                var c = chars[i];

                if (!_full && (c == '\r' || c == '\n'))
                    sb.Append(c);
                else if (char.IsControl(c))
                    sb.AppendFormat("${0:X2}", (byte)c);
                else if (c == '$')
                    sb.Append("$$");
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public override byte[] Unescape(byte[] bytes)
        {
            List<byte> bb = new();
            for (int i = 0; i < bytes.Length; i++)
            {
                var c = bytes[i];
                if (c == 0) continue;

                if (c == '$')
                {
                    if (bytes[i + 1] == '$')
                    {
                        bb.Add((byte)'$');
                        i++;
                    }
                    else
                    {
                        string hex = string.Concat((char)bytes[i + 1], (char)bytes[i + 2]);
                        try
                        {
                            bb.Add(Convert.ToByte(hex, 16));
                            i += 2;
                        }
                        catch
                        {
                            bb.Add(c);
                        }
                    }
                }
                else
                    bb.Add(c);
            }

            return bb.ToArray();
        }
    }

    public class SlashEscaper : BaseEscaper
    {
        internal SlashEscaper() { }

        public override string Escape(char[] chars)
        {
            var a = "\x001234";
            StringBuilder sb = new();

            for (int i = 0; i < chars.Length; i++)
            {
                if (char.IsControl(chars[i]))
                {
                    sb.Append('\\').Append(GetEscape((byte)chars[i]));
                }
                else if (chars[i] == '\\')
                    sb.Append("\\\\");
                else
                    sb.Append(chars[i]);
            }
            return sb.ToString();
        }

        public override byte[] Unescape(byte[] bytes)
        {
            List<byte> bb = new();
            for (int c = 0; c < bytes.Length; c++)
            {
                if (bytes[c] == 0) continue;

                if (bytes[c] == '\\')
                {
                    if (bytes[c + 1] == '\\')
                    {
                        bb.Add((byte)'\\');
                        c++;
                    }
                    else
                    {
                        bb.Add(GetUnescape(bytes, ref c));
                    }
                }
                else
                {
                    bb.Add(bytes[c]);
                }
            }

            return bb.ToArray();
        }

        private static string GetEscape(byte val) => val switch
        {
            0x00 => "0",
            0x09 => "t",
            0x0a => "n",
            0x0d => "r",
            _ => $"x{val:x4}"
        };

        private static byte GetUnescape(byte[] arr, ref int index)
        {
            var c = (char)arr[++index];
            if (c == 'x')
            {
                ++index;
                var hex = Encoding.ASCII.GetString(arr, index, 4);
                index += 3;
                return (byte)Convert.ToInt32(hex, 16);
            }

            return c switch
            {
                '0' => 0x00,
                't' => 0x09,
                'n' => 0x0a,
                'r' => 0x0d,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
