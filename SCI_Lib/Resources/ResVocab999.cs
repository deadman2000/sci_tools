using SCI_Lib.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SCI_Lib.Resources
{
    public class ResVocab999 : ResVocab
    {
        private string[] _text;

        /// <summary>
        /// \0 terminated lines
        /// </summary>
        /// <returns></returns>
        public string[] GetText()
        {
            return _text ??= Read();
        }

        private string[] Read()
        {
            var data = GetContent();

            try
            {
                var lines = ReadIndicies(data);
                if (lines != null) return lines;
            }
            catch { }

            return ReadText(data);
        }

        private static string[] ReadIndicies(byte[] data)
        {
            var ms = new MemoryStream(data);
            int cnt = ms.ReadUShortBE();
            if (cnt > 0xff) return null;

            string[] lines = new string[cnt];
            ushort[] offsets = new ushort[cnt];

            for (int i = 0; i < cnt; i++)
                offsets[i] = ms.ReadUShortBE();

            for (int i = 0; i < cnt; i++)
            {
                ms.Position = offsets[i];
                var len = ms.ReadUShortBE();
                lines[i] = Encoding.ASCII.GetString(ms.ReadBytes(len));
            }

            return lines;
        }

        private string[] ReadText(byte[] data)
        {
            List<string> lines = new();

            int s = 0;
            int ind = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0x00)
                {
                    string strRes = GameEncoding.GetString(data, s, i - s);
                    lines.Add(strRes);
                    ind++;
                    s = i + 1;
                }
            }

            return lines.ToArray();
        }
    }
}
