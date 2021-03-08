namespace SCI_Lib.Resources.Scripts.Elements
{
    public class OpCode
    {
        public OpCode(ushort type, string name)
        {
            Type = type;
            Name = name;
        }

        public ushort Type { get; private set; }

        public string Name { get; private set; }
    }
}
