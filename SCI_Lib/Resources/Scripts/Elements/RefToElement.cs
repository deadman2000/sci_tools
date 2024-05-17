using SCI_Lib.Utils;
using System;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public class RefToElement : BaseElement
    {
        /// <summary>
        /// Общий конструктор
        /// </summary>
        /// <param name="script"></param>
        /// <param name="value"></param>
        /// <param name="targetOffset">Указатель</param>
        /// <param name="size">Число байт в значении</param>
        RefToElement(BaseScript script, ushort addr, ushort value, ushort targetOffset, byte size, bool relative)
            : base(script, addr)
        {
            RefScript = script;
            Value = value;
            TargetOffset = targetOffset;
            Size = size;
            Relative = relative;
            if (relative)
                Shift = targetOffset - addr - value;
        }

        public RefToElement(BaseScript script, ushort addr, ushort value, ushort targetOffset, byte size)
            : this(script, addr, value, targetOffset, size, true)
        {
        }

        /// <summary>
        /// Конструктор для абсолютной ссылки
        /// </summary>
        /// <param name="script"></param>
        /// <param name="value"></param>
        public RefToElement(BaseScript script, ushort addr, ushort value)
            : this(script, addr, value, value, 2, false)
        {
        }

        public BaseScript RefScript { get; set; }

        public ushort Value { get; }

        public ushort TargetOffset { get; set; }

        public bool Relative { get; }

        public int Shift { get; }

        public byte Size { get; }

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
                case Code:
                    type = "code";
                    break;
                case StringConst s:
                    type = "string";
                    comment = ";  '" + s.GetStringEscape() + "'";
                    break;
                case PropertyElement p:
                    type = "prop";
                    comment = $";  ${p.ValueStr}";
                    break;
                case SaidExpression s:
                    return s.ToString();
                case RefToElement r:
                    type = "ref_ref";
                    comment = $";  {r.Address:x04}";
                    break;
                case null:
                    type = "null";
                    break;
                default:
                    type = "ref_el";
                    comment = $";  {Reference}";
                    break;
            }

            return $"{type}_{TargetOffset:x4}{comment}";
        }

        // Обновляет ссылку на элемент
        public override void SetupByOffset()
        {
            IsSetup = true;
            Reference?.XRefs.Remove(this);
            Reference = RefScript.GetElement(TargetOffset);
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
            IsOffsetWrited = true;

            if (Reference != null)
                TargetOffset = Reference.Address;

            int val = TargetOffset;
            if (Relative)
                val -= Address + Shift;

            switch (Size)
            {
                case 1: bb.SetByte(Address, (byte)val); break;
                case 2: bb.SetShortBE(Address, (short)val); break;
                default: throw new NotImplementedException();
            }
        }

        public string ToHex(int rel)
        {
            if (Reference != null)
                TargetOffset = Reference.Address;

            int val = TargetOffset;
            if (Relative)
                val -= Address + Shift;

            return Size switch
            {
                1 => $"{val:x2}",
                2 => $"{val:x4}",
                _ => throw new NotImplementedException(),
            };
        }
    }

    public class ExportRef : RefToElement
    {
        public ExportRef(BaseScript script, ushort addr, ushort value)
            : base(script, addr, value)
        {
        }
    }


    public class FuncRef : RefToElement
    {
        public FuncRef(BaseScript script, ushort addr, ushort value)
            : base(script, addr, value)
        {
        }
    }

    public class CodeRef : RefToElement
    {
        public CodeRef(Code code, ushort addr, ushort value, ushort targetOffset, byte size)
            : base(code.Owner, addr, value, targetOffset, size)
        {
            Source = code;
        }

        protected override void WriteData(ByteBuilder bb)
        {
            base.WriteData(bb);
        }
    }
}
