using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Resources.Scripts1_1;
using SCI_Lib.Resources.View;
using SCI_Lib.Resources.Vocab;
using SCI_Lib.SCI0;
using SCI_Lib.SCI1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SCI_Lib
{
    public abstract class SCIPackage
    {
        public static SCIPackage Load(string directory, Encoding enc = null)
        {
            string mapFile = Path.Combine(directory, "RESOURCE.MAP");
            if (IsSCI0(mapFile))
                return new SCI0Package(directory, enc);
            else
                return new SCI1Package(directory, enc);
        }

        private static bool IsSCI0(string mapFile)
        {
            using FileStream fs = File.OpenRead(mapFile);
            var test = new byte[6];
            fs.Seek(-6, SeekOrigin.End);
            fs.Read(test, 0, 6);
            fs.Seek(0, SeekOrigin.Begin);
            return test.All(b => b == 0xff);
        }

        private Palette _palette;

        public Palette GlobalPalette => _palette ??= GetGlobalPalette();

        private Palette GetGlobalPalette()
        {
            return GetResource<ResPalette>(999).GetPalette();
        }

        public string GetResFileName(Resource resource) => GetResFileName(resource.Type, resource.Number);

        public abstract string GetResFileName(ResType type, int number);

        public SCIPackage(string directory, Encoding enc)
        {
            if (enc == null)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                enc = Encoding.GetEncoding(866);
            }
            GameEncoding = new GameEncoding(enc);

            GameDirectory = directory;

            ReadMap(Path.Combine(GameDirectory, "RESOURCE.MAP"));

            var messagesPath = Path.Combine(GameDirectory, "MESSAGE.MAP");
            if (File.Exists(messagesPath))
            {
                ExternalMessages = true;
                ReadMap(messagesPath);
            }

            Resources = Resources
                .Distinct()
                .OrderBy(r => r.Type).ThenBy(r => r.Number).ToList();

            SeparateHeapResources = Resources.Any(r => r.Type == ResType.Heap);
        }

        public abstract ResourceFileInfo LoadResourceInfo(string resourceFileName, int offset);

        private void ReadMap(string mapPath)
        {
            using FileStream fs = File.OpenRead(mapPath);
            ReadMap(fs);
        }

        protected abstract void ReadMap(FileStream fs);

        protected abstract void SaveMap(FileStream fs);

        public Resource CreateResource(ResType type, ushort num)
        {
            var res = CreateRes(type, num);
            res.Init(this, type, num, 1, -1);
            Resources.Add(res);
            return res;
        }

        protected virtual Resource CreateRes(ResType type, ushort num)
        {
            return type switch
            {
                ResType.Text => new ResText(),
                ResType.Vocabulary => CreateVocab(num),
                ResType.Script => new ResScript(),
                ResType.Font => new ResFont(),
                ResType.Message => new ResMessage(),
                ResType.Picture => new ResPicture(),
                ResType.View => new ResView(),
                ResType.Palette => new ResPalette(),
                ResType.Heap => new ResHeap(),
                _ => new Resource(),
            };
        }

        private static Resource CreateVocab(ushort num)
        {
            return num switch
            {
                0 => new ResVocab000(),
                1 => new ResVocab001(),
                900 => new ResVocab900(),
                901 => new ResVocab901(),
                997 => new ResVocab997(),
                998 => new ResVocab998(),
                999 => new ResVocab999(),
                _ => new ResVocab()
            };
        }

        private IEnumerable<IScript> _scriptsCache;

        public ClassSection GetClassSection(ushort id)
        {
            _scriptsCache ??= Scripts.Select(r => r.GetScript());

            foreach (var s in _scriptsCache.OfType<Script>())
            {
                var cls = s.GetClassSection(id);
                if (cls != null) return cls;
            }
            return null;
        }

        public Object1_1 GetObject(ushort id)
        {
            _scriptsCache ??= Scripts.Select(r => r.GetScript());

            foreach (var s in _scriptsCache.OfType<Script1_1>())
            {
                var cls = s.GetObject(id);
                if (cls != null) return cls;
            }
            return null;
        }

        public GameEncoding GameEncoding { get; private set; }

        public string GameDirectory { get; }

        public bool SeparateHeapResources { get; set; }

        public bool ExternalMessages { get; set; }

        private ViewFormat _viewFormat = ViewFormat.NotSet;
        public ViewFormat ViewFormat
        {
            get
            {
                if (_viewFormat == ViewFormat.NotSet) DetectViewFormat();
                return _viewFormat;
            }
        }

        private void DetectViewFormat()
        {
            var data = GetResources<ResView>().First().GetContent();

            try
            {
                var view = new SCIView(this);
                view.ReadVGA(data);
                _viewFormat = ViewFormat.VGA;
                return;
            }
            catch
            {
            }

            try
            {
                var view = new SCIView(this);
                view.ReadVGA11(data);
                _viewFormat = ViewFormat.VGA1_1;
                return;
            }
            catch
            {
            }

            try
            {
                var view = new SCIView(this);
                view.ReadEGA(data);
                _viewFormat = ViewFormat.EGA;
                return;
            }
            catch
            {
            }

            _viewFormat = ViewFormat.Unknown;
        }

        #region Resources

        public List<Resource> Resources { get; } = new List<Resource>();

        public IEnumerable<ResText> Texts => GetResources<ResText>();

        public IEnumerable<ResScript> Scripts => GetResources<ResScript>();

        public IEnumerable<ResMessage> Messages => GetResources<ResMessage>();

        public IEnumerable<Resource> GetResources(ResType resType) => Resources.Where(r => r.Type == resType);

        public IEnumerable<T> GetResources<T>() where T : Resource => Resources.Where(r => r is T).Cast<T>();

        public Resource GetResource(ResType type, ushort number)
        {
            var res = Resources.FirstOrDefault(r => r.Type == type && r.Number == number);
            if (res != null) return res;

            if (File.Exists(Path.Combine(GameDirectory, GetResFileName(type, number))))
                return CreateResource(type, number);
            return null;
        }

        public Resource GetResource(string fileName) => Resources.FirstOrDefault(r => r.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));

        public T GetResource<T>(ushort number) where T : Resource => Resources.FirstOrDefault(r => r is T && r.Number == number) as T;

        public Resource Get(Resource res) => Resources.FirstOrDefault(r => r.Type == res.Type && r.Number == res.Number);

        public T Get<T>(T res) where T : Resource => GetResource<T>(res.Number);

        #endregion

        private Dictionary<byte, OpCode> _opcodes;

        public string GetOpCodeName(byte type)
        {
            _opcodes ??= LoadOpCodes();
            if (_opcodes == null) return null;
            return _opcodes[type]?.Name;
        }

        private Dictionary<byte, OpCode> LoadOpCodes() => GetResource<ResVocab998>(998)?.GetVocabOpcodes();

        private string[] _funcNames;

        public string GetFuncName(int ind)
        {
            _funcNames ??= LoadFuncs();
            if (_funcNames == null) return null;
            if (ind >= _funcNames.Length) return $"kernel_{ind}";
            return _funcNames[ind];
        }

        private string[] LoadFuncs() => GetResource<ResVocab999>(999)?.GetText();

        private string[] _names;

        public string GetName(int ind)
        {
            _names ??= LoadNames();
            if (_names == null) return null;
            if (ind >= _names.Length) return $"unknown{ind}";
            return _names[ind];
        }

        private string[] LoadNames() => GetResource<ResVocab997>(997)?.GetVocabNames();

        public ClassSection GetClass(ushort id)
        {
            foreach (var res in Scripts)
            {
                var scr = res.GetScript() as Script;
                foreach (var cl in scr.Get<ClassSection>())
                    if (cl.Type == SectionType.Class && cl.Id == id)
                        return cl;
            }
            return null;
        }

        public IEnumerable<Resource> GetTextResources()
        {
            return GetResources(ResType.Text)
                .Union(GetResources(ResType.Script))
                .Union(GetResources(ResType.Message))
                .Union(GetResources(ResType.Heap));
        }

        public void Pack(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var volumes = Resources.SelectMany(r => r.Volumes).Select(r => r.Num).Distinct().OrderBy(r => r);
            var all = Resources.SelectMany(r => r.Volumes.Select(rf => new { res = r, num = rf.Num, off = rf.Offset }));

            foreach (var num in volumes)
            {
                var resName = $"RESOURCE.{num:D3}";
                Console.WriteLine(resName);

                var resources = all.Where(r => r.num == num).OrderBy(r => r.off);

                var filePath = Path.Combine(directory, resName);
                using var fs = File.OpenWrite(filePath);

                foreach (var r in resources)
                {
                    r.res.Pack(fs, r.num);
                }
            }

            Console.WriteLine("RESOURCE.MAP");
            using (FileStream fs = File.OpenWrite(Path.Combine(directory, "RESOURCE.MAP")))
            {
                SaveMap(fs);
            }
        }


        private Dictionary<ushort, string> _idToWord;
        private Dictionary<string, ushort[]> _wordId;

        public Dictionary<ushort, string> GetWords() => _idToWord ??= ReadIdToWords();
        public Dictionary<string, ushort[]> GetWordIds() => _wordId ??= ReadWordsToId();

        private Dictionary<ushort, string> ReadIdToWords()
        {
            if (GetResource<ResVocab>(0) is not ResVocab000 voc) return null;
            IEnumerable<Word> words = voc.GetWords();

            if (GetResource(ResType.Vocabulary, 1) is ResVocab001 vocTr)
                words = words.Union(vocTr.GetWords());

            return words.GroupBy(w => w.Group).ToDictionary(g => g.Key, g => GetTranslated(g));
        }

        private Dictionary<string, ushort[]> ReadWordsToId()
        {
            if (GetResource<ResVocab>(0) is not ResVocab000 voc) return null;
            IEnumerable<Word> words = voc.GetWords();

            if (GetResource(ResType.Vocabulary, 1) is ResVocab001 vocTr)
                words = words.Union(vocTr.GetWords());

            return words.GroupBy(w => w.Text).ToDictionary(g => g.Key, g => g.Select(s => s.Group).Distinct().ToArray());
        }

        public ushort[] GetWordId(string word)
        {
            if (GetWordIds().TryGetValue(word, out var id)) return id;
            return null;
        }

        public void ResetWords()
        {
            _idToWord = null;
            _wordId = null;
        }

        private static string GetTranslated(IEnumerable<Word> words)
        {
            var word = words.FirstOrDefault(w => w.Text[0] > 'z');
            word ??= words.First();
            return word.Text;
        }
    }
}
