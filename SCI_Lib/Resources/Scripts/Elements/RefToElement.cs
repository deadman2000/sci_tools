using SCI_Lib.Utils;
using System;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public class RefToElement : BaseElement
    {
        /// <summary>
        /// Конструктор для относительной ссылки
        /// </summary>
        /// <param name="script"></param>
        /// <param name="value"></param>
        /// <param name="targetOffset"></param>
        /// <param name="size"></param>
        public RefToElement(Script script, ushort addr, ushort value, ushort targetOffset, byte size)
            : base(script, addr)
        {
            Value = value;
            TargetOffset = targetOffset;
            Size = size;
            Relative = true;
        }

        /// <summary>
        /// Конструктор для абсолютной ссылки
        /// </summary>
        /// <param name="script"></param>
        /// <param name="value"></param>
        public RefToElement(Script script, ushort addr, ushort value)
            : this(script, addr, value, value, 2)
        {
            Relative = false;
        }

        public ushort Value { get; }

        public ushort TargetOffset { get; set; }

        public bool Relative { get; }

        public byte Size { get; }

        public int Index { get; set; }

        public BaseElement Reference { get; set; }

        public bool IsSetup { get; private set; }

        public bool IsWrited { get; private set; }

        public bool IsOffsetWrited { get; private set; }

        public object Source { get; set; }

        public override string ToString()
        {
            string type;
            string comment = "";

            switch (Reference)
            {
                case Code c:
                    type = "code";
                    comment = ";  " + c.Name;
                    break;
                case StringConst s:
                    type = "string";
                    comment = ";  '" + s.GetStringEscape() + "'";
                    break;
                case PropertyElement p:
                    type = "prop";
                    comment = $";  ${p.ValueStr:x4}";
                    break;
                case SaidExpression s:
                    return s.ToString();
                case RefToElement r:
                    type = "ref_ref";
                    comment = $";  {r.Address:x4}";
                    break;
                case null:
                    type = "null";
                    break;
                default:
                    type = "ref_el";
                    break;

            }

            return $"{type}_{TargetOffset:x4}{comment}";
        }

        public override void SetupByOffset()
        {
            IsSetup = true;
            Reference?.XRefs.Remove(this);
            Reference = Script.GetElement(TargetOffset);
            Reference?.XRefs.Add(this);
        }

        protected override void WriteData(ByteBuilder bb)
        {
            IsWrited = true;
            switch (Size)
            {
                case 1: bb.AddByte(0); break;
                case 2: bb.AddShortBE(0); break;
                default: throw new NotImplementedException();
            }
        }

        public override void WriteOffset(ByteBuilder bb)
        {
            WriteOffset(0, bb);
        }

        public void WriteOffset(int rel, ByteBuilder bb)
        {
            IsOffsetWrited = true;

            if (Reference != null)
                TargetOffset = Reference.Address;

            int val = TargetOffset;
            if (Relative)
                val -= rel;

            switch (Size)
            {
                case 1: bb.SetByte(Address, (byte)val); break;
                case 2: bb.SetShortBE(Address, (ushort)val); break;
                default: throw new NotImplementedException();
            }
        }

        public string ToHex(int rel)
        {
            int val;

            if (Reference != null)
                val = Reference.Address;
            else
                val = TargetOffset;

            if (Relative)
                val -= rel;

            switch (Size)
            {
                case 1: return $"{val:x2}";
                case 2: return $"{val:x4}";
                default: throw new NotImplementedException();
            }
        }
    }

    public class ExportRef : RefToElement
    {
        public ExportRef(Script script, ushort addr, ushort value)
            : base(script, addr, value)
        {
        }
    }


    public class FuncRef : RefToElement
    {
        public FuncRef(Script script, ushort addr, ushort value)
            : base(script, addr, value)
        {
        }
    }

    public class CodeRef : RefToElement
    {
        private Code _code;

        public CodeRef(Code code, ushort addr, ushort value, ushort targetOffset, byte size)
            : base(code.Script, addr, value, targetOffset, size)
        {
            Source = _code = code;
        }

        public override void SetupByOffset()
        {
            base.SetupByOffset();
            /*if (!(Reference is Code))
                throw new Exception($"{_code.Script.Resource} {_code}");*/
        }
    }
}
