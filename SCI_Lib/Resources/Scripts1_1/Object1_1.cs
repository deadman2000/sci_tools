using SCI_Lib.Resources.Scripts;
using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace SCI_Lib.Resources.Scripts1_1
{
    public class Object1_1 : IClass
    {
        private readonly byte[] _heap;
       
        private ushort[] _functionSelectors;
        private ushort[] _functionOffsets;

        private ushort[] _propertySelectors;

        private readonly ushort[] _generalProps = new ushort[8];
        private Dictionary<string, LocalVar> _props;
        private Dictionary<ushort, LocalVar> _propsInt;
        private LocalVar[] _propertyValues;

        private bool _isInstance;

        public bool InstanceOf(string className)
        {
            var s = Super;
            while (s != null)
            {
                if (s.Name == className) return true;
                s = s.Super;
            }
            return false;
        }

        public Object1_1(Script1_1 script, byte[] heap, ushort offset)
        {
            Script = script;
            _heap = heap;
            Offset = offset;
        }

        public Script1_1 Script { get; }

        public ushort Offset { get; }

        public int? ExportInd { get; set; }

        public Dictionary<string, LocalVar> Properties => _props ??= PrepareProps();

        public Dictionary<ushort, LocalVar> PropsInt => _propsInt ??= PreparePropsInt();

        public bool TryGetProperty(string name, out ushort value)
        {
            if (Properties.TryGetValue(name, out var val))
            {
                value = val.Value;
                return true;
            }
            value = default;
            return false;
        }

        public bool HasProperty(string key) => Properties.ContainsKey(key);

        public ushort this[GeneralProperty prop] => _generalProps[(int)prop];

        public ushort this[string property]
        {
            get
            {
                if (!Properties.TryGetValue(property, out var value)) throw new ArgumentException($"Property {property} does not exists");
                return value.Value;
            }
        }

        public ushort Id => this[GeneralProperty.ObjectId];

        public ushort ClassId => this[GeneralProperty.Script];

        private Object1_1 _super;
        public Object1_1 Super
        {
            get
            {
                if (_super != null) return _super;

                var porpSuper = this[GeneralProperty.Super];
                if (porpSuper == 0xFFFF) return null;

                return _super = Script.Package.GetClass(porpSuper) as Object1_1;
            }
        }

        private string _name;
        public string Name => _name ??= GetString(this["name"]);

        public override string ToString() => Name;

        public void Read(MemoryStream stream, MemoryStream heap)
        {
            var numVars = heap.ReadUShortBE();
            var varOffset = heap.ReadUShortBE();
            var methodsOffset = heap.ReadUShortBE();
            var mystery = heap.ReadUShortBE();
            if (mystery != 0) throw new Exception("Wrong script format");

            // Read properties
            stream.Seek(varOffset, SeekOrigin.Begin);
            _propertySelectors = new ushort[numVars];
            for (int i = 0; i < numVars; i++)
            {
                _propertySelectors[i] = stream.ReadUShortBE();
            }

            _propertyValues = new LocalVar[numVars];
            for (int i = 0; i < numVars; i++)
            {
                if (i < 5)
                    _generalProps[i] = 0;
                else if (i < 8)
                    _generalProps[i] = heap.ReadUShortBE();
                else
                {
                    bool isString = Script.StringOffsets.Contains((ushort)heap.Position);
                    var w = heap.ReadUShortBE();
                    _propertyValues[i] = new LocalVar { Value = w, IsObjectOrString = isString };
                }
            }

            _isInstance = this[GeneralProperty.Script] == 0xffff;

            // Methods
            stream.Seek(methodsOffset, SeekOrigin.Begin);
            var numMethods = stream.ReadUShortBE();
            _functionSelectors = new ushort[numMethods];
            _functionOffsets = new ushort[numMethods];
            for (int i = 0; i < numMethods; i++)
            {
                _functionSelectors[i] = stream.ReadUShortBE();
                _functionOffsets[i] = stream.ReadUShortBE();
            }
        }

        private Dictionary<string, LocalVar> PrepareProps()
        {
            var dict = new Dictionary<string, LocalVar>();
            ushort[] sel = _isInstance ? Super._propertySelectors : _propertySelectors;

            for (int i = 8; i < _propertyValues.Length; i++)
            {
                var name = Script.Package.GetName(sel[i]);
                dict[name] = _propertyValues[i];
            }

            return dict;
        }

        private Dictionary<ushort, LocalVar> PreparePropsInt()
        {
            var dict = new Dictionary<ushort, LocalVar>();
            ushort[] sel = _isInstance ? Super._propertySelectors : _propertySelectors;

            for (int i = 8; i < _propertyValues.Length; i++)
            {
                dict[sel[i]] = _propertyValues[i];
            }

            return dict;
        }

        private string GetString(ushort offset)
        {
            return Script.Package.GameEncoding.GetString(_heap, offset);
        }
    }
}
