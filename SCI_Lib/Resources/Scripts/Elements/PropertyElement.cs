using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Utils;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public class PropertyElement : BaseElement
    {
        private readonly ClassSection _class;
        private readonly int _index;

        public ushort Value { get; set; }
        public BaseElement Reference { get; set; }

        public string ValueStr => Reference != null ? Reference.ToString() : $"{Value:x}";

        public override string Label => $"{_class.Name}[{_index}] = {ValueStr}";

        public PropertyElement(ClassSection cl, int index, ushort address, ushort val)
            : base(cl.Script, address)
        {
            Value = val;
            _class = cl;
            _index = index;
        }

        protected override void WriteData(ByteBuilder bb)
        {
            if (Reference != null)
                Value = Reference.Address;

            bb.AddUShortBE(Value);
        }

        public override void WriteOffset(ByteBuilder bb)
        {
        }

        public override string ToString() => $"${Value:x}";
    }
}
