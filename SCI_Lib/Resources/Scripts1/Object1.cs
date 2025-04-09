using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;
using System;
using System.IO;
using System.Linq;

namespace SCI_Lib.Resources.Scripts1
{
    public class Object1 : BaseElement, IScriptInstance
    {
        private ushort _propOffset;
        private ushort _methodsOffset;
        private bool _isInstance;

        public Method[] Methods { get; private set; }

        public Property[] Properties { get; private set; }

        public Object1(Heap heap, ushort offset)
            : base(heap, offset)
        {
        }

        public Heap Heap => (Heap)Owner;

        public Script1 Script { get; set; }

        public ushort ClassId => Properties[5].Value;

        private Object1 _super;

        public Object1 Super
        {
            get
            {
                if (_super != null) return _super;

                var porpSuper = Properties[6].Value;
                if (porpSuper == 0xFFFF) return null;

                return _super = Heap.Package.GetObject(porpSuper);
            }
        }

        public string Name => Properties[8].StringValue.Value;

        public override string ToString() => Name;

        public void ReadHeap(MemoryStream heap)
        {
            var propsCount = heap.ReadUShortBE();
            _propOffset = heap.ReadUShortBE();
            _methodsOffset = heap.ReadUShortBE();
            var mystery = heap.ReadUShortBE();
            if (mystery != 0) throw new Exception("Wrong script format");

            _isInstance = _propOffset == _methodsOffset;

            // Properties
            Properties = new Property[propsCount];
            for (int i = 0; i < 5; i++)
                Properties[i] = new Property();

            for (int i = 5; i < Properties.Length; i++)
            {
                ushort pos = (ushort)heap.Position;
                var val = heap.ReadUShortBE();

                StringConst str = Heap.GetStringByOffset(pos, val);
                Properties[i] = new Property { Value = val, StringValue = str };
            }
        }

        public void ReadScript(Script1 scr, MemoryStream stream)
        {
            Script = scr;

            // Local variables
            int varsCount = (_methodsOffset - _propOffset) / 2;
            if (varsCount > 0)
            {
                stream.Seek(_propOffset, SeekOrigin.Begin);
                for (int i = 0; i < varsCount; i++)
                {
                    var p = Properties[i];
                    p.Selector = stream.ReadUShortBE();
                    p.Name = Owner.Package.GetName(p.Selector);
                }
            }

            // Methods
            stream.Seek(_methodsOffset, SeekOrigin.Begin);
            var methodsCount = stream.ReadUShortBE();
            Methods = new Method[methodsCount];
            for (int i = 0; i < methodsCount; i++)
            {
                var sel = stream.ReadUShortBE();

                var pos = stream.Position;
                var offset = stream.ReadUShortBE();
                var r = new GlobalRef(Script, (ushort)pos, offset);
                Methods[i] = new Method(this, sel, offset, r);
            }
        }

        protected override void WriteData(ByteBuilder bb) // Heap
        {
            bb.AddUShortBE(0x1234); // magic
            bb.AddUShortBE((ushort)Properties.Length);
            bb.AddUShortBE(_propOffset); // Properties addr in script
            bb.AddUShortBE(_methodsOffset); // Method addr in script
            bb.AddShortBE(0); // always zero

            for (int i = 5; i < Properties.Length; i++)
                bb.AddUShortBE(Properties[i].Value);
        }

        public void WriteScriptHead(ByteBuilder bb)
        {
            _propOffset = (ushort)bb.Position;
            if (!_isInstance)
            {
                foreach (var p in Properties)
                {
                    bb.AddUShortBE(p.Selector);
                }
            }

            _methodsOffset = (ushort)bb.Position;
            bb.AddUShortBE((ushort)Methods.Length);
            foreach (var m in Methods)
            {
                bb.AddUShortBE(m.Selector);
                bb.AddUShortBE(0); // m.Offset
            }
        }

        public Property GetPropertyByName(string name)
        {
            Prepare();
            return Properties.FirstOrDefault(p => p.Name == name);
        }

        public void Prepare()
        {
            if (ClassId != 0xffff) return;

            if (Properties.Any(p => p.Name == null))
            {
                for (int i = 0; i < Properties.Length; i++)
                {
                    Properties[i].Name = Super.Properties[i].Name;
                }
            }
        }

        public ushort GetProperty(string name) => GetPropertyByName(name).Value;

        public void SetProperty(string name, ushort value)
        {
            GetPropertyByName(name).Value = value;
        }
    }
}
