using SCI_Lib.Utils;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public class ShortArg : BaseElement
    {
        private Code _code;

        public short Value { get; set; }

        public ShortArg(Code code, ushort address, short value) : base(code.Owner, address)
        {
            _code = code;
            Value = value;
        }

        protected override void WriteData(ByteBuilder bb) => bb.AddShortBE(Value);

        public override string ToString() => $"{Value:x04}";
    }
}
