using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SCI_Lib.Resources.Scripts
{
    public class Script : IScript
    {
        private readonly Dictionary<ushort, BaseElement> _elements = new Dictionary<ushort, BaseElement>();
        private readonly StringSection _strings;

        public Script(Resource res)
        {
            Resource = res;
            
            SourceData = res.GetContent();

            ushort i = 0;
            while (i < SourceData.Length)
            {
                SectionType type = (SectionType)Helpers.GetUShortBE(SourceData, i);
                if (type == SectionType.None) break;

                ushort size = Helpers.GetUShortBE(SourceData, i + 2); // Size with header
                i += 4; // Skip header
                size -= 4;
                Section sec = Section.Create(this, type, SourceData, i, size);
                Sections.Add(sec);
                i += size;

                if (sec is StringSection section)
                    _strings = section;
            }

            foreach (var sec in Sections)
                sec.SetupByOffset();
        }


        public void Register(BaseElement el) => _elements[el.Address] = el;

        public SCIPackage Package { get { return Resource.Package; } }

        public Resource Resource { get; }

        public byte[] SourceData { get; }

        public List<Section> Sections { get; } = new List<Section>();

        public IEnumerable<StringConst> AllStrings() => Sections.OfType<StringSection>().SelectMany(s => s.Strings);

        public IEnumerable<BaseElement> AllElements => _elements.Values.Where(e => !(e is StringPart));

        public IEnumerable<RefToElement> AllRefs => _elements.Values.OfType<RefToElement>();

        public BaseElement GetElement(ushort offset)
        {
            if (_elements.TryGetValue(offset, out var el)) return el;
            return null;
        }

        public byte[] GetBytes()
        {
            ByteBuilder bb = new ByteBuilder();

            foreach (Section sec in Sections)
            {
                bb.AddUShortBE((ushort)sec.Type);
                int sizePos = bb.Position;
                bb.AddShortBE(0);
                sec.Write(bb);
                int endPos = bb.Position;
                bb.SetShortBE(sizePos, (ushort)(endPos - sizePos + 2));
            }

            foreach (Section sec in Sections)
                sec.WriteOffsets(bb);

            bb.AddShortBE(0);
            return bb.GetArray();
        }

        public StringPart GetStringPart(ushort value)
        {
            if (_strings == null) return null;

            foreach (var s in _strings.Strings)
                if (s.Address < value && value < s.Address + s.Bytes.Length)
                    return new StringPart(s, s.Address - value);
            return null;
        }

        public List<T> Get<T>() where T : Section
        {
            List<T> list = new List<T>();

            foreach (Section sec in Sections)
            {
                if (sec is T t)
                    list.Add(t);
            }

            return list;
        }

        internal List<T> Get<T>(SectionType type) where T : Section
        {
            List<T> list = new List<T>();

            foreach (Section sec in Sections)
            {
                if (sec.Type == type && sec is T)
                    list.Add((T)sec);
            }

            return list;
        }

        public IClass GetClass(ushort id)
        {
            return Get<ClassSection>(SectionType.Class).Find(c => c.Id == id);
        }

        public string GetOpCodeName(byte type) => Package.GetOpCodeName(type);

        public Section CreateSection(SectionType type)
        {
            Section sec = Section.Create(this, type, SourceData, 0, 0);
            Sections.Add(sec);
            return sec;
        }
    }
}
