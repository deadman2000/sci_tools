using SCI_Lib.Resources.Scripts.Elements;
using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Resources.Scripts
{
    public abstract class BaseScript
    {
        private readonly Dictionary<ushort, BaseElement> _elements = new();

        public Resource Resource { get; }

        public SCIPackage Package => Resource.Package;

        public BaseScript(Resource res)
        {
            Resource = res;
        }

        public abstract byte[] GetBytes();

        public abstract IScriptInstance GetInstance(string name);

        public abstract IScriptInstance GetInstance(string name, string superName);

        public abstract IEnumerable<StringConst> AllStrings();

        #region Elements

        public void Register(BaseElement el)
        {
            if (el is StringPart) return;
            _elements[el.Address] = el;
        }

        public void Unregister(BaseElement el) => _elements.Remove(el.Address);

        public IEnumerable<BaseElement> AllElements => _elements.Values;

        public IEnumerable<RefToElement> AllRefs => _elements.Values.OfType<RefToElement>();

        public BaseElement GetElement(ushort offset)
        {
            if (_elements.TryGetValue(offset, out var el)) return el;
            return null;
        }

        public Code GetOperator(ushort address)
        {
            if (_elements.TryGetValue(address, out var el) && el is Code code)
                return code;
            return null;
        }

        #endregion

        public string GetOpCodeName(byte type) => Package.GetOpCodeName(type);

    }
}
