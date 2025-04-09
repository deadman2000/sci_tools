using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;
using System;

namespace SCI_Lib.Resources.Scripts.Sections
{
    public class LocalVariablesSection : Section
    {
        public override void Read(byte[] data, ushort offset, int length)
        {
            Vars = new object[length / 2];

            for (int i = 0; i < Vars.Length; i++)
            {
                Vars[i] = ReadUShortBE(data, ref offset);
            }
        }

        public object[] Vars { get; private set; }

        public int Count => Vars.Length;

        public ushort this[int index] => Vars[index] switch
        {
            ushort us => us,
            BaseRef r => r.Address,
            _ => throw new Exception(),
        };

        public override void SetupByOffset()
        {
            for (int i = 0; i < Vars.Length; i++)
            {
                ushort val = (ushort)Vars[i];
                var el = _script.GetElement(val);
                if (el is StringConst || el is SaidExpression)
                {
                    var r = new GlobalRef(_script, (ushort)(Address + i * 2), val);
                    r.SetupByOffset();
                    Vars[i] = r;
                }
            }
        }

        public override void Write(ByteBuilder bb)
        {
            foreach (object v in Vars)
            {
                switch (v)
                {
                    case ushort s:
                        bb.AddUShortBE(s);
                        break;
                    case BaseRef r:
                        r.Write(bb);
                        break;
                }
            }
        }
    }
}
