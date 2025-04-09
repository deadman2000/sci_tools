using SCI_Lib.Utils;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public abstract class BaseRef : BaseElement
    {
        public BaseRef(BaseScript script, ushort addr)
            : base(script, addr)
        {
        }

        /// <summary>
        /// Абсолютный адрес цели
        /// </summary>
        public ushort TargetOffset { get; set; }

        /// <summary>
        /// Элемент цели
        /// </summary>
        public BaseElement Reference { get; set; }

        public bool IsSetup { get; protected set; }

        public bool IsWrited { get; protected set; }

        public bool IsOffsetWrited { get; protected set; }

        public bool CanBeInvalid { get; set; }

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
                case BaseRef r:
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
            Reference = Owner.GetElement(TargetOffset);
            Reference?.XRefs.Add(this);
        }

        public abstract string ToHex();
    }

    public class RelativeByteRef : BaseRef
    {
        public RelativeByteRef(BaseScript script, ushort addr, sbyte value, short shift = 1)
            : base(script, addr)
        {
            Value = value;
            Shift = shift;
            TargetOffset = (ushort)(addr + value + shift);
        }

        public sbyte Value { get; private set; }

        public short Shift { get; }

        protected override void WriteData(ByteBuilder bb)
        {
            IsWrited = true;
            bb.AddByte(0);
        }

        public override void WriteOffset(ByteBuilder bb)
        {
            IsOffsetWrited = true;

            if (Reference != null)
                TargetOffset = Reference.Address;

            Value = (sbyte)(TargetOffset - Address - Shift);

            bb.SetSByte(Address, Value);
        }

        public override string ToHex() => $"{Value:x02}";
    }

    public class RelativeWordRef : BaseRef
    {
        public RelativeWordRef(BaseScript script, ushort addr, short value, short shift)
            : base(script, addr)
        {
            Value = value;
            Shift = shift;
            TargetOffset = (ushort)(addr + value + shift);
        }

        public short Value { get; private set; }
        public short Shift { get; }

        protected override void WriteData(ByteBuilder bb)
        {
            IsWrited = true;
            bb.AddShortBE(0);
        }

        public override void WriteOffset(ByteBuilder bb)
        {
            IsOffsetWrited = true;

            if (Reference != null)
                TargetOffset = Reference.Address;

            Value = (short)(TargetOffset - Address - Shift);

            bb.SetShortBE(Address, Value);
        }

        public override string ToHex() => $"{Value:x04}";
    }

    public class GlobalRef : BaseRef
    {
        public GlobalRef(BaseScript script, ushort addr, ushort target)
            : base(script, addr)
        {
            TargetOffset = target;
        }

        protected override void WriteData(ByteBuilder bb)
        {
            IsWrited = true;
            bb.AddShortBE(0);
        }

        public override void WriteOffset(ByteBuilder bb)
        {
            IsOffsetWrited = true;

            if (Reference != null)
                TargetOffset = Reference.Address;

            bb.SetUShortBE(Address, TargetOffset);
        }

        public override string ToHex() => $"{TargetOffset:x04}";
    }
}
