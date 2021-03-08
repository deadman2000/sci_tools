using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;

namespace SCI_Lib.Resources.Scripts.Sections
{
    public class ClassSection : Section
    {
        ushort funcList;
        ushort[] varselectors;

        public override void Read(byte[] data, ushort offset, int length)
        {
            var magic = ReadShortBE(data, ref offset);
            if (magic != 0x1234)
                throw new System.Exception("Wrong class magic");

            var varOffset = ReadShortBE(data, ref offset);
            if (varOffset != 0)
                throw new System.Exception("Wrong class var offset");

            funcList = ReadShortBE(data, ref offset);
            int selectorsCount = ReadShortBE(data, ref offset);

            Selectors = new BaseElement[selectorsCount];
            for (int i = 0; i < selectorsCount; i++)
            {
                var addr = offset;
                var val = ReadShortBE(data, ref offset);
                Selectors[i] = new ShortElement(_script, addr, val);
            }

            if (Type == SectionType.Class)
            {
                varselectors = new ushort[selectorsCount];
                for (int i = 0; i < selectorsCount; i++)
                    varselectors[i] = ReadShortBE(data, ref offset);
            }

            int fs = ReadShortBE(data, ref offset);
            FuncNames = new ushort[fs];
            for (int i = 0; i < fs; i++)
                FuncNames[i] = ReadShortBE(data, ref offset);

            offset += 2;

            FuncCode = new FuncRef[fs];
            for (int i = 0; i < fs; i++)
            {
                var addr = offset;
                FuncCode[i] = new FuncRef(_script, addr, ReadShortBE(data, ref offset)) { Source = this };
            }
        }

        public override void SetupByOffset()
        {
            for (int i = 0; i < Selectors.Length; i++)
            {
                ShortElement c = (ShortElement)Selectors[i];
                var val = c.Value;

                var target = _script.GetElement(val);
                if (target != null && target is StringConst) //  || (target is StringPart)
                {
                    var r = new RefToElement(_script, c.Address, c.Value) { Source = this };
                    c.ReplaceBy(r);
                    Selectors[i] = r;
                    r.SetupByOffset();
                }
            }

            {
                if (Selectors[3] is ShortElement s)
                {
                    StringPart p = _script.GetStringPart(s.Value);
                    if (p != null)
                    {
                        _script.Register(p);
                        var r = new RefToElement(_script, s.Address, s.Value) { Source = this };
                        s.ReplaceBy(r);
                        Selectors[3] = r;
                        r.SetupByOffset();
                    }
                }
            }

            var nameRef = (Selectors[3] as RefToElement)?.Reference;
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

            foreach (var r in FuncCode)
                r.SetupByOffset();
        }

        public ushort Id => ((ShortElement)Selectors[0]).Value;

        public string Name { get; private set; }

        public BaseElement[] Selectors { get; private set; }

        public ushort[] Varselectors => varselectors ?? SuperClass.Varselectors;

        public ushort[] FuncNames { get; private set; }

        public FuncRef[] FuncCode { get; private set; }

        private ClassSection _superClass;

        public ClassSection SuperClass
        {
            get
            {
                if (_superClass != null) return _superClass;
                return _superClass = Package.GetClass(((ShortElement)Selectors[1]).Value);
            }
        }

        public override void Write(ByteBuilder bb)
        {
            bb.AddShortBE(0x1234);
            bb.AddShortBE(0);
            bb.AddShortBE(funcList);
            bb.AddShortBE((ushort)Selectors.Length);

            for (int i = 0; i < Selectors.Length; i++)
                Selectors[i].Write(bb);

            if (Type == SectionType.Class)
            {
                foreach (ushort vs in varselectors)
                    bb.AddShortBE(vs);
            }

            bb.AddShortBE((ushort)FuncNames.Length);
            foreach (ushort val in FuncNames)
                bb.AddShortBE(val);

            bb.AddShortBE(0);

            foreach (RefToElement r in FuncCode)
                r.Write(bb);
        }

        public override void WriteOffsets(ByteBuilder bb)
        {
            for (int i = 0; i < Selectors.Length; i++)
                Selectors[i].WriteOffset(bb);

            foreach (RefToElement r in FuncCode)
                r.WriteOffset(bb);
        }
    }
}
