using SCI_Lib.Utils;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public class ShortElement : BaseElement
    {
        public ShortElement(Script script, ushort address, ushort val)
            : base(script, address)
        {
            Value = val;
        }

        public ushort Value { get; set; }

        protected override void WriteData(ByteBuilder bb)
        {
            bb.AddUShortBE(Value);
        }

        public override void WriteOffset(ByteBuilder bb)
        {
        }

        public override string ToString() => $"${Value:x}";
    }
}
