using SCI_Lib.Utils;
using System.Collections.Generic;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public abstract class BaseElement
    {
        private ushort address;

        public BaseElement(Script script, ushort address)
        {
            Script = script;
            Address = address;
            //if (Address == 0x14e)                Console.WriteLine();

            Script.Register(this);
        }

        public Script Script { get; private set; }

        public GameEncoding GameEncoding => Script.Package.GameEncoding;

        public List<RefToElement> XRefs { get; } = new List<RefToElement>();

        public virtual ushort Address
        {
            get => address; set
            {
                address = value;
                IsAddressSet = true;
            }
        }

        public bool IsAddressSet { get; set; }

        public virtual string Label => $"{GetType().Name.ToLower()}_{Address:x4}";

        public virtual void SetupByOffset() { }

        public abstract void Write(ByteBuilder bb);

        public abstract void WriteOffset(ByteBuilder bb);

        public void ReplaceBy(BaseElement el)
        {
            foreach (var r in XRefs)
                r.Reference = el;

            el.XRefs.AddRange(XRefs);
        }
    }
}
