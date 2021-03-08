using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;

namespace SCI_Lib.Resources.Scripts.Sections
{
    class LocalVariablesSection : Section
    {
        public override void Read(byte[] data, ushort offset, int length)
        {
            Vars = new object[length / 2];

            for (int i = 0; i < Vars.Length; i++)
            {
                Vars[i] = ReadShortBE(data, ref offset);
            }
        }

        public object[] Vars { get; private set; }

        public override void SetupByOffset()
        {
            for (int i = 0; i < Vars.Length; i++)
            {
                ushort val = (ushort)Vars[i];
                var el = _script.GetElement(val);
                if (el is StringConst)
                {
                    var r = new RefToElement(_script, (ushort)(Address + i * 2), val) { Source = this };
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
                        bb.AddShortBE(s);
                        break;
                    case RefToElement r:
                        r.Write(bb);
                        break;
                }
            }
        }

        public override void WriteOffsets(ByteBuilder bb)
        {
            foreach (var v in Vars)
                (v as RefToElement)?.WriteOffset(bb);
        }
    }

    class LocalVariablesSection2 : Section
    {
        public override void Read(byte[] data, ushort offset, int length)
        {
            Refs = new ushort[length / 2];

            for (int i = 0; i < Refs.Length; i++)
            {
                Refs[i] = ReadShortBE(data, ref offset);
            }
        }

        public ushort[] Refs { get; private set; }

        public override void Write(ByteBuilder bb)
        {
            foreach (var r in Refs)
            {
                bb.AddShortBE(r);
            }
        }
    }
}
