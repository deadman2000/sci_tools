using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;
using System.Collections.Generic;

namespace SCI_Lib.Resources
{
    public class ResVocab : Resource
    {
        private string[] _text;
        private string[] _vocabNames;
        private Dictionary<byte, OpCode> _opcodes;

        /// <summary>
        /// \0 terminated lines
        /// </summary>
        /// <returns></returns>
        public string[] GetText()
        {
            if (_text != null)
                return _text;

            List<string> lines = new List<string>();
            var data = GetContent();

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

            return _text = lines.ToArray();
        }

        public string[] GetVocabNames()
        {
            if (_vocabNames != null)
                return _vocabNames;

            var data = GetContent();

            ushort count = Helpers.GetUShortBE(data, 0);
            string[] names = new string[count];

            for (int i = 0; i < count; i++)
            {
                ushort addr = Helpers.GetUShortBE(data, i * 2 + 2);
                ushort len = Helpers.GetUShortBE(data, addr);
                string name = GameEncoding.GetStringEscape(data, addr + 2, len);
                names[i] = name;
            }

            return _vocabNames = names;
        }

        public Dictionary<byte, OpCode> GetVocabOpcodes()
        {
            if (_opcodes != null) return _opcodes;

            var data = GetContent();

            ushort count = Helpers.GetUShortBE(data, 0);
            Dictionary<byte, OpCode> opcodes = new Dictionary<byte, OpCode>();
            for (byte i = 0; i < count; i++)
            {
                ushort addr = Helpers.GetUShortBE(data, i * 2 + 2);
                ushort len = Helpers.GetUShortBE(data, addr);
                ushort type = Helpers.GetUShortBE(data, addr + 2);
                string name = GameEncoding.GetStringEscape(data, addr + 4, len - 2);
                opcodes.Add(i, new OpCode(type, name));
            }

            return _opcodes = opcodes;
        }
    }
}
