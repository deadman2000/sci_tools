using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;
using System;

namespace SCI_Lib.Resources.Scripts.Sections
{
    class ExportSection : Section
    {
        public override void Read(byte[] data, ushort offset, int length)
        {
            int cnt = ReadShortBE(data, ref offset);
            Exports = new ExportRef[cnt];
            for (int i = 0; i < cnt; i++)
            {
                var addr = offset;
                var val = ReadShortBE(data, ref offset);
                var zero = ReadShortBE(data, ref offset);
                if (zero != 0)
                    throw new Exception();

                if (val != 0)
                    Exports[i] = new ExportRef(_script, addr, val) { Source = this };
            }
        }

        public ExportRef[] Exports { get; private set; }

        public override void SetupByOffset()
        {
            foreach (ExportRef exp in Exports)
                exp?.SetupByOffset();
        }

        public override void Write(ByteBuilder bb)
        {
            bb.AddShortBE((ushort)Exports.Length);
            foreach (ExportRef exp in Exports)
            {
                if (exp != null)
                    exp.Write(bb);
                else
                    bb.AddShortBE(0);
                bb.AddShortBE(0);
            }
        }

        public override void WriteOffsets(ByteBuilder bb)
        {
            foreach (ExportRef exp in Exports)
                exp?.WriteOffset(bb);
        }
    }
}
