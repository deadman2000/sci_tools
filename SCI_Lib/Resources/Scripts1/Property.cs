using SCI_Lib.Resources.Scripts.Elements;

namespace SCI_Lib.Resources.Scripts1
{
    public class Property
    {
        public ushort Value { get; set; }
        public StringConst StringValue { get; set; }

        public string Name { get; set; }
        public ushort Selector { get; set; }

        public override string ToString() => StringValue?.Value ?? $"{Value:X04}";
    }
}
