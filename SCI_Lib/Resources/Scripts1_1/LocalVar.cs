namespace SCI_Lib.Resources.Scripts1_1
{
    class LocalVar
    {
        public ushort Value { get; set; }

        public bool IsObjectOrString { get; set; }

        public override string ToString() => $"value = {Value}; IsObjectOrString = {IsObjectOrString}";
    }
}
