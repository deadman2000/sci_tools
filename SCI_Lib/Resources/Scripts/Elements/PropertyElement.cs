using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Utils;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public class PropertyElement : BaseElement
    {
        public ClassSection Class { get; }
        public int Index { get; }

        private int _address;

        public string Name { get; set; }
        public ushort Value { get; set; }
        public BaseElement Reference { get; set; }
        public string ValueStr => Reference != null ? Reference.ToString() : $"{Value:x}";
        public override string Label => $"{Class.Name}[{Index}] = {ValueStr}";

        public ushort NameSel { get; internal set; }

        public PropertyElement(ClassSection cl, int index, ushort address, ushort val)
            : base(cl.Script, address)
        {
            Value = val;
            Class = cl;
            Index = index;
        }

        protected override void WriteData(ByteBuilder bb)
        {
            _address = bb.Position;
            bb.AddUShortBE(0);
        }

        public override void WriteOffset(ByteBuilder bb)
        {
            if (Value == 0x1e86)
                System.Console.WriteLine();

            if (Reference != null)
                Value = Reference.Address;

            bb.SetUShortBE(_address, Value);
        }

        public override string ToString() => $"{Name} = ${Value:x}";
    }
}
