using SCI_Lib.Resources.Scripts.Assembler;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;
using System.Collections.Generic;

namespace SCI_Lib.Resources.Scripts.Sections
{
    public class CodeSection : Section
    {
        public override void Read(byte[] data, ushort offset, int length)
        {
            Operators = new List<Code>();
            ushort i = offset;

            Code prev = null;

            while (i < offset + length)
            {
                Code c = new(this, i, prev);
                c.Read(data, ref i);
                Operators.Add(c);
                prev = c;
            }
        }

        public List<Code> Operators { get; private set; }

        public override void SetupByOffset()
        {
            foreach (Code c in Operators)
                c.SetupByOffset();
        }

        public override void Write(ByteBuilder bb)
        {
            foreach (Code c in Operators)
                c.Write(bb);
        }

        public override void WriteOffsets(ByteBuilder bb)
        {
            foreach (Code c in Operators)
                c.WriteOffset(bb);
        }

        public ushort ReplaceASM(string asm)
        {
            Clear();
            return AppendASM(asm);
        }

        public ushort AppendASM(string asm)
        {
            var lines = asm.Split('\r', '\n');

            string currentLabel = null;
            List<AsmOperator> asmOps = new();
            Dictionary<string, AsmOperator> labels = new();

            foreach (var line in lines)
            {
                var l = line.Trim();
                var commentInd = line.IndexOf(';');
                if (commentInd >= 0)
                    l = line[..commentInd].Trim();

                if (l.Length == 0) continue;

                if (l[^1] == ':')
                {
                    currentLabel = l[..^1];
                    continue;
                }

                var op = AsmOperator.Parse(l);
                asmOps.Add(op);
                if (currentLabel != null)
                {
                    labels.Add(currentLabel, op);
                    currentLabel = null;
                }
            }

            var begin = Address;
            foreach (var op in Operators)
                begin += op.Size;

            var addr = (ushort)begin;
            Code prev = null;
            foreach (var op in asmOps)
            {
                var code = op.GetCode(this, addr, prev);
                Operators.Add(code);

                addr = (ushort)(addr + code.Size);
                prev = code;
            }

            foreach (var op in asmOps)
                op.SetupLabels(labels);

            return (ushort)begin;
        }

        private void Clear()
        {
            foreach (var op in Operators)
            {
                Script.Unregister(op);
                foreach (var a in op.Arguments)
                {
                    if (a is BaseElement e)
                        Script.Unregister(e);
                }
            }
            Operators.Clear();
        }
    }
}
