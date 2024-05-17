using SCI_Lib.Utils;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public class ByteArg : BaseElement
    {
        private Code _code;

        public byte Value { get; set; }

        public ByteArg(Code code, ushort address, byte value) : base(code.Owner, address)
        {
            _code = code;
            Value = value;
        }

        protected override void WriteData(ByteBuilder bb) => bb.AddByte(Value);

        public override string ToString() => $"{Value:x02}";
    }
}
