using SCI_Lib.Resources.Scripts.Elements;
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
                case "pushi":
                    return ParseBW(0x38, parts[1]);
                case "bt":
                    return new BranchOperator(0x2e, parts[1]);
                case "bnt":
                    return new BranchOperator(0x30, parts[1]);
                case "sub":
                    return new AsmOperator(0x04);
                case "ge?":
                    return new AsmOperator(0x20);
                case "le?":
                    return new AsmOperator(0x24);
                case "ret":
                    return new AsmOperator(0x48);
                case "pprev":
                    return new AsmOperator(0x60);

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

        internal Code GetCode(ICodeBlock block, ushort address, Code prev)
        {
            Code = new Code(block, address, prev)
            {
                Type = _type,
            };
            Code.SetArguments(_args);
            return Code;
        }

        public virtual void SetupLabels(Dictionary<string, AsmOperator> labels)
        {
        }
    }

    internal class BranchOperator : AsmOperator
    {
        private string _targetRef;

        public BranchOperator(byte op, string targetRef)
            : base(op, (ushort)0)
        {
            _targetRef = targetRef;
        }

        public override void SetupLabels(Dictionary<string, AsmOperator> labels)
        {
            if (!labels.TryGetValue(_targetRef, out var op))
                throw new Exception($"Label {_targetRef} not found");

            ((ShortArg)Code.Arguments[0]).Value = (short)(op.Code.Address - (Code.Address + Code.Size));
        }
    }
}
