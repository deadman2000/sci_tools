using SCI_Lib.Analyzer;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Utils;
using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Resources.Scripts
{
    public class Script : IScript
    {
        private readonly Dictionary<ushort, BaseElement> _elements = new();
        private StringSection _strings;

        public Script(Resource res)
        {
            Resource = res;
            SourceData = res.GetContent();
            Read();
        }

        public Script(Resource res, byte[] data)
        {
            Resource = res;
            SourceData = data;
            Read();
        }

        private void Read()
        {
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

        public SaidSection SaidSection => Sections.OfType<SaidSection>().FirstOrDefault();

        public StringSection StringSection => Sections.OfType<StringSection>().FirstOrDefault();
        
        public SynonymSecion SynonymSecion => Sections.OfType<SynonymSecion>().FirstOrDefault();

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
            ByteBuilder bb = new();

            foreach (Section sec in Sections)
            {
                bb.AddUShortBE((ushort)sec.Type);
                int sizePos = bb.Position;
                bb.AddShortBE(0);
                sec.Write(bb);
                int endPos = bb.Position;
                bb.SetUShortBE(sizePos, (ushort)(endPos - sizePos + 2));
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

        public IEnumerable<T> Get<T>() where T : Section => Sections.OfType<T>();

        internal IEnumerable<T> Get<T>(SectionType type) where T : Section => Sections.OfType<T>().Where(s => s.Type == type);

        public ClassSection GetInstance(string name) => Get<ClassSection>().FirstOrDefault(c => c.Name == name);
        
        public ClassSection GetClassSection(ushort id) => Get<ClassSection>(SectionType.Class).FirstOrDefault(c => c.Id == id);

        public string GetOpCodeName(byte type) => Package.GetOpCodeName(type);

        public Section CreateSection(SectionType type)
        {
            Section sec = Section.Create(this, type, SourceData, 0, 0);
            Sections.Add(sec);
            return sec;
        }

        public Code GetOperator(ushort address)
        {
            if (_elements.TryGetValue(address, out var el) && el is Code code)
                return code;
            return null;
        }

        public ScriptAnalyzer Analyze(string cl = null, string method = null) => new(this, cl, method);
    }
}
