using System.Collections.Generic;
using SCI_Lib.Utils;

namespace SCI_Lib.Resources.Scripts.Sections
{
    public class SaidSection : Section
    {
        List<byte[]> _saids = new List<byte[]>();

        public override void Read(byte[] data, ushort offset, int length)
        {
            var buff = new List<byte>();
            for (int i = 0; i < length; i++)
            {
                var val = data[offset + i];
                if (val == 0xff)
                {
                    _saids.Add(buff.ToArray());
                    buff.Clear();
                }
                else
                {
                    if (val >= 0xf0)
                        buff.Add(val); // Operator
                    else
                    {
                        buff.Add(val); // Word
                        buff.Add(data[offset + i + 1]);
                        i++;
                    }
                }
            }
        }

        public override void Write(ByteBuilder bb)
        {
            foreach (var data in _saids)
            {
                bb.AddBytes(data);
                bb.AddByte(0xff);
            }
            if (bb.Position % 2 == 1)
                bb.AddByte(0);
        }
    }
}