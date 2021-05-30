namespace SCI_Lib.Resources.Scripts1_1
{
    public class LocalVar
    {
        public ushort Value { get; set; }

        public bool IsObjectOrString { get; set; }

        public override string ToString() {
            if (IsObjectOrString) 
                return $"*{Value:X04}";
            else
                return $"{Value:X04}";
        }
    }
}
