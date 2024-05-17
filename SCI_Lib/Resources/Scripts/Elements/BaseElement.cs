using SCI_Lib.Utils;
using System.Collections.Generic;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public abstract class BaseElement
    {
        protected ushort _address;

        public BaseElement(BaseScript script, ushort address)
        {
            Owner = script;
            _address = address;

            Owner.Register(this);
        }

        public BaseScript Owner { get; private set; }

        public GameEncoding GameEncoding => Owner.Package.GameEncoding;

        public List<RefToElement> XRefs { get; } = new List<RefToElement>();

        public ushort Address => _address;

        public virtual string Label => $"{GetType().Name.ToLower()}_{Address:x4}";

        public virtual void SetupByOffset() { }

        public void Write(ByteBuilder bb)
        {
            _address = (ushort)bb.Position;
            WriteData(bb);
        }

        protected abstract void WriteData(ByteBuilder bb);

        public virtual void WriteOffset(ByteBuilder bb) { }

        public void ReplaceBy(BaseElement el)
        {
            foreach (var r in XRefs)
                r.Reference = el;

            el.XRefs.AddRange(XRefs);
        }

        public void ResetAddress()
        {
            _address = 0;
        }
    }
}
