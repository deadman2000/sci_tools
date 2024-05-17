using SCI_Lib.Utils;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public class ExportElement : BaseElement
    {
        public ExportElement(BaseScript script, ushort address, ushort exportOffset) : base(script, address)
        {
            Offset = exportOffset;
        }

        public ushort Offset { get; private set; }

        protected override void WriteData(ByteBuilder bb)
        {
            bb.AddUShortBE(Offset);
        }

        public override string ToString() => $"{Offset:x04}";
    }
}
