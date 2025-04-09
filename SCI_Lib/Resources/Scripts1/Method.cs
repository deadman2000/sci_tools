using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;

namespace SCI_Lib.Resources.Scripts1
{
    public class Method /*: ICodeBlock*/
    {
        private readonly Object1 _object;
        private string _name;

        public ushort Selector { get; }
        public ushort Address { get; set; }
        public BaseRef Reference { get; }
        public BaseScript CodeOwner => _object.Script;

        public Method(Object1 obj, ushort sel, ushort offset, BaseRef r)
        {
            _object = obj;
            Selector = sel;
            Address = offset;
            Reference = r;
        }

        public string Name
        {
            get
            {
                if (_name != null) return _name;
                _name = _object.Script.Package.GetName(Selector);
                return _name;
            }
        }

        public override string ToString() => $"{Name} : {Address:x04}";
    }
}
