using System.Collections.Generic;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;

namespace SCI_Lib.Resources.Scripts.Sections
{
    public class SaidSection : Section
    {
        public List<SaidExpression> Saids { get; set; }

        public override void Read(byte[] data, ushort offset, int length)
        {
            Saids = new List<SaidExpression>();
            var buff = new List<SaidData>();
            ushort address = offset;
            for (int i = 0; i < length; i++)
            {
                var val = data[offset + i];
                if (val == 0xff)
                {
                    var said = new SaidExpression(_script, address, buff.ToArray());
                    Saids.Add(said);
                    buff.Clear();
                    address = (ushort)(offset + i + 1);
                    continue;
                }

                if (val >= 0xf0)
                {
                    buff.Add(new SaidData(val)); // Operator
                }
                else
                {
                    ushort off = (ushort)(offset + i);
                    var word = ReadShortLE(data, ref off);
                    buff.Add(new SaidData(word));
                    i++;
                }
            }
        }

        public override void Write(ByteBuilder bb)
        {
            foreach (var said in Saids)
            {
                said.Write(bb);
            }
            if (bb.Position % 2 == 1)
                bb.AddByte(0);
        }
    }
}