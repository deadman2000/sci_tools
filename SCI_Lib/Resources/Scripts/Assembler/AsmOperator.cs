using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SCI_Lib.Resources.Scripts.Assembler
{
    internal class AsmOperator
    {
        protected byte _type;
        protected object[] _args;
        public Code Code { get; private set; }

        protected AsmOperator(byte type, params object[] args)
        {
            _type = type;
            _args = args;
        }

        public static AsmOperator Parse(string code)
        {
            var parts = code.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var op = parts[0];

            switch (op)
            {
                case "lsp":
                    return ParseBW(0x8e, parts[1]);
                case "lap":
                    return ParseBW(0x86, parts[1]);
                case "ldi":
                    return ParseBW(0x34, parts[1]);
                case "bnt":
                    return new BntOperator(parts[1]);
                case "sub":
                    return new AsmOperator(0x04);
                case "ge?":
                    return new AsmOperator(0x20);
                case "le?":
                    return new AsmOperator(0x24);
                case "ret":
                    return new AsmOperator(0x48);

                default:
                    throw new Exception($"Unknown or not yet supported operator {code}");
            }
        }

        private static AsmOperator ParseBW(byte type, string argStr)
        {
            var a = int.Parse(argStr, NumberStyles.HexNumber);
            object arg;
            if (a > 0x7f)
            {
                arg = (ushort)a;
            }
            else
            {
                arg = (byte)a;
                type |= 1;
            }
            return new AsmOperator(type, arg);
        }

        internal Code GetCode(CodeSection section, ushort address, Code prev)
        {
            return Code = new Code(section, address, prev)
            {
                Type = _type,
                Arguments = new List<object>(_args)
            };
        }

        public virtual void SetupLabels(Dictionary<string, AsmOperator> labels)
        {
        }
    }

    internal class BntOperator : AsmOperator
    {
        private string _targetRef;

        public BntOperator(string targetRef)
            : base(0x30, (ushort)0)
        {
            _targetRef = targetRef;
        }

        public override void SetupLabels(Dictionary<string, AsmOperator> labels)
        {
            if (!labels.TryGetValue(_targetRef, out var op))
                throw new Exception($"Label {_targetRef} not found");

            var val = (ushort)(op.Code.Address - (Code.Address + Code.Size));
            Code.Arguments[0] = val;
        }
    }
}
