using System;
using System.Text;

namespace SCI_Lib.Utils
{
    public static class Helpers
    {
        public static byte[] GetBytes(byte[] data, int offset, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, offset, result, 0, length);
            return result;
        }

        public static int GetIntBE(byte[] data, int offset)
        {
            return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
        }

        public static ushort GetUShortBE(byte[] data, int offset)
        {
            return (ushort)(data[offset] | (data[offset + 1] << 8));
        }
        
        public static string ByteToHex(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(String.Format("{0:X2} ", data[i]));
            }
            return sb.ToString().TrimEnd();
        }

        public static string ByteToHex(byte[] data, int offset, int len)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = offset; i < offset + len && i < data.Length; i++)
            {
                sb.Append(String.Format("{0:X2} ", data[i]));
            }
            return sb.ToString().TrimEnd();
        }

    }
}
