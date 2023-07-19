using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Utils;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public class ClassRef : BaseElement
    {
        private readonly ClassSection _class;

        public ClassRef(ClassSection cl)
            : base(cl.Script, cl.Selectors[0].Address)
        {
            _class = cl;
        }

        public override void WriteOffset(ByteBuilder bb)
        {
        }

        protected override void WriteData(ByteBuilder bb)
        {
        }

        public override string Label => _class.Name;
    }
}
