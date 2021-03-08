using SCI_Lib.Utils;
using System;
using System.Collections.Generic;

namespace SCI_Lib.Resources
{
    public class ResText : Resource
    {
        public override string[] GetStrings()
        {
            List<string> lines = new List<string>();
            var data = GetContent();

            int s = 0;
            int ind = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0x00)
                {
                    lines.Add(GameEncoding.GetString(data, s, i - s));

                    ind++;
                    s = i + 1;
                }
            }

            return lines.ToArray();
        }

        public override void SetStrings(string[] strings)
        {
            var oldStrings = GetStrings();

            if (strings.Length != oldStrings.Length)
                throw new Exception("Line count mismatch");

            ByteBuilder bb = new ByteBuilder();

            for (int r = 0; r < strings.Length; r++)
            {
                var tr = strings[r];
                if (tr == null) tr = oldStrings[r];

                var bytes = GameEncoding.GetBytes(tr);
                bb.AddBytes(bytes);
                bb.AddByte(0);
            }

            SavePatch(bb.GetArray());
        }
    }
}
