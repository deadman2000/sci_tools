using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;
using System.Collections.Generic;

namespace SCI_Lib.Resources
{
    public class ResVocab998 : ResVocab
    {
        private Dictionary<byte, OpCode> _opcodes;
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
