using SCI_Lib.Utils;
using System;
using System.Collections.Generic;

namespace SCI_Lib.Resources
{
    public class ResText : Resource
    {
        private string[] _strings;

        public override string[] GetStrings()
        {
            if (_strings != null) return _strings;

            List<string> strings = new List<string>();
            var data = GetContent();

            int s = 0;
            int ind = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0x00)
                {
                    strings.Add(GameEncoding.GetString(data, s, i - s));

                    ind++;
                    s = i + 1;
                }
            }

            return _strings = strings.ToArray();
        }

        public override void SetStrings(string[] strings)
        {
            var resStrings = GetStrings();

            if (strings.Length != resStrings.Length)
                throw new Exception("Line count mismatch");

            for (int i = 0; i < strings.Length; i++)
            {
                var tr = strings[i];
                if (tr != null) resStrings[i] = tr;
            }
        }

        public override byte[] GetPatch()
        {
            var strings = GetStrings();
         
            ByteBuilder bb = new ByteBuilder();

            for (int i = 0; i < strings.Length; i++)
            {
                var bytes = GameEncoding.GetBytes(strings[i]);
                bb.AddBytes(bytes);
                bb.AddByte(0);
            }

            return bb.GetArray();
        }
    }
}
