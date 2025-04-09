using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;
using System;

namespace SCI_Lib.Resources.Scripts.Sections
{
    public class RelocationSection : Section
    {
        private bool _additionalZero;

        public GlobalRef[] Refs { get; private set; }

        public override void Read(byte[] data, ushort offset, int length)
        {
            int cnt = ReadUShortBE(data, ref offset);
            if (length / 2 == cnt + 2)
                _additionalZero = true;
            else if (length / 2 == cnt + 1)
                _additionalZero = false;
            else
                throw new FormatException();

            Refs = new GlobalRef[cnt];

            if (_additionalZero)
                offset += 2;
            for (int i = 0; i < cnt; i++)
            {
                var addr = offset;
                ushort val = ReadUShortBE(data, ref offset);
                var el = new GlobalRef(_script, addr, val);
                Refs[i] = el;
            }
        }

        public override void Write(ByteBuilder bb)
        {
            _offset = bb.Position;
            bb.AddUShortBE((ushort)Refs.Length);
            if (_additionalZero)
                bb.AddShortBE(0);
            for (int i = 0; i < Refs.Length; i++)
                Refs[i].Write(bb);
        }
    }
}
