using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;
using System;
using System.Linq;

namespace SCI_Lib.Resources.Scripts.Sections
{
    public class ClassSection : Section
    {
        public ushort Id => Properties[0].Value;

        private ushort funcList;
        private ushort[] _propNamesInd;

        public string Name { get; private set; }

        public PropertyElement[] Properties { get; private set; }

        public ushort[] PropNamesInd => _propNamesInd ?? SuperClass.PropNamesInd;

        public ushort[] FuncNamesInd { get; private set; }
        public string[] FuncNames { get; private set; }

        public FuncRef[] FuncCode { get; private set; }

        private ClassSection _superClass;
        private bool _prepared;

        public ClassSection SuperClass => _superClass ??= Package.GetClassSection(Properties[1].Value);


        public override void Read(byte[] data, ushort offset, int length)
        {
            var magic = ReadShortBE(data, ref offset);
            if (magic != 0x1234)
                throw new Exception("Wrong class magic");

            var varOffset = ReadShortBE(data, ref offset);
            if (varOffset != 0)
                throw new Exception("Wrong class var offset");

            funcList = ReadShortBE(data, ref offset);
            int propsCount = ReadShortBE(data, ref offset);

            Properties = new PropertyElement[propsCount];
            for (int i = 0; i < propsCount; i++)
            {
                var addr = offset;
                var val = ReadShortBE(data, ref offset);
                Properties[i] = new PropertyElement(this, i, addr, val);
            }

            if (Type == SectionType.Class)
            {
                _propNamesInd = new ushort[propsCount];
                for (int i = 0; i < propsCount; i++)
                {
                    var selector = ReadShortBE(data, ref offset);
                    _propNamesInd[i] = selector;
                    Properties[i].Name = Package.GetName(selector);
                    Properties[i].NameSel = selector;
                }
            }

            int fs = ReadShortBE(data, ref offset);
            FuncNamesInd = new ushort[fs];
            FuncNames = new string[fs];
            for (int i = 0; i < fs; i++)
            {
                var ind = ReadShortBE(data, ref offset);
                FuncNamesInd[i] = ind;
                FuncNames[i] = Package.GetName(ind);
            }

            offset += 2;

            FuncCode = new FuncRef[fs];
            for (int i = 0; i < fs; i++)
            {
                var addr = offset;
                FuncCode[i] = new FuncRef(_script, addr, ReadShortBE(data, ref offset)) { Source = this };
            }

            //_script.Register(Selectors[0]);
        }

        public void Prepare()
        {
            if (_prepared) return;
            _prepared = true;
            if (Type == SectionType.Object)
            {
                for (int i = 0; i < Properties.Length; i++)
                {
                    if (SuperClass.Properties.Length <= i)
                        Properties[i].Name = $"prop_{i}";
                    else
                    {
                        Properties[i].Name = SuperClass.Properties[i].Name;
                        Properties[i].NameSel = SuperClass.Properties[i].NameSel;
                    }
                }
            }
        }

        public override void SetupByOffset()
        {
            for (int i = 0; i < Properties.Length; i++)
            {
                var c = Properties[i];
                var val = c.Value;

                var target = _script.GetElement(val);
                if (target != null && (target is StringConst || target is SaidExpression)) //  || (target is StringPart)
                {
                    c.Reference = target;
                }
            }

            if (Properties.Length > 3)
            {
                var nameRef = Properties[3].Reference;
                if (nameRef != null)
                {
                    if (nameRef is StringConst s)
                    {
                        Name = s.Value;
                        s.IsClassName = true;
                    }
                    else if (nameRef is StringPart p)
                    {
                        Name = p.String;
                        p.OrigString.IsClassName = true;
                    }
                }
            }

            foreach (var r in FuncCode)
                r.SetupByOffset();
        }

        public override void Write(ByteBuilder bb)
        {
            bb.AddShortBE(0x1234);
            bb.AddShortBE(0);
            bb.AddUShortBE(funcList);
            bb.AddUShortBE((ushort)Properties.Length);

            for (int i = 0; i < Properties.Length; i++)
                Properties[i].Write(bb);

            if (Type == SectionType.Class)
            {
                foreach (ushort vs in _propNamesInd)
                    bb.AddUShortBE(vs);
            }

            bb.AddUShortBE((ushort)FuncNamesInd.Length);
            foreach (ushort val in FuncNamesInd)
                bb.AddUShortBE(val);

            bb.AddShortBE(0);

            foreach (RefToElement r in FuncCode)
                r.Write(bb);
        }

        public override void WriteOffsets(ByteBuilder bb)
        {
            for (int i = 0; i < Properties.Length; i++)
                Properties[i].WriteOffset(bb);

            foreach (RefToElement r in FuncCode)
                r.WriteOffset(bb);
        }

        public bool IsProp(string name) => Properties.Any(p => p.Name == name);

        public bool IsProp(ushort sel) => Properties.Any(p => p.NameSel == sel);
    }
}
