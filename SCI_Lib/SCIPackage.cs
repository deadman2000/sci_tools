using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.View;
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

        public abstract string GetResFileName(Resource resource);

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

            Resources = Resources.OrderBy(r => r.Type).ThenBy(r => r.Number).ToList();

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

        protected virtual Resource CreateRes(ResType type)
        {
            return type switch
            {
                ResType.Text => new ResText(),
                ResType.Vocabulary => new ResVocab(),
                ResType.Script => new ResScript(),
                ResType.Font => new ResFont(),
                ResType.Message => new ResMessage(),
                ResType.Picture => new ResPicture(),
                ResType.View => new ResView(),
                ResType.Palette => new ResPalette(),
                _ => new Resource(),
            };
        }

        private IEnumerable<IScript> _scriptsCache;

        public IClass GetClass(ushort id)
        {
            if (_scriptsCache == null)
                _scriptsCache = Scripts.Select(r => r.GetScript());

            foreach (var s in _scriptsCache)
            {
                var cls = s.GetClass(id);
                if (cls != null) return cls;
            }
            return null;
        }

        public GameEncoding GameEncoding { get; private set; }

        public string GameDirectory { get; }

        public bool SeparateHeapResources { get; set; }

        public bool ExternalMessages { get; set; }

        public ViewFormats ViewFormat { get; set; } = ViewFormats.NotSet;

        #region Resources

        public List<Resource> Resources { get; } = new List<Resource>();

        public IEnumerable<ResText> Texts => GetResources<ResText>();

        public IEnumerable<ResScript> Scripts => GetResources<ResScript>();

        public IEnumerable<ResMessage> Messages => GetResources<ResMessage>();

        public IEnumerable<Resource> GetResources(ResType resType) => Resources.FindAll(r => r.Type == resType);

        public IEnumerable<T> GetResources<T>() where T : Resource => Resources.FindAll(r => r is T).Cast<T>();

        public Resource GetResource(ResType type, ushort number) => Resources.FirstOrDefault(r => r.Type == type && r.Number == number);

        public Resource GetResource(string fileName) => Resources.FirstOrDefault(r => r.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));

        public T GetResource<T>(ushort number) where T : Resource => Resources.FirstOrDefault(r => r is T && r.Number == number) as T;

        public T Get<T>(T res) where T : Resource => GetResource<T>(res.Number);

        #endregion

        private Dictionary<byte, OpCode> _opcodes;

        public string GetOpCodeName(byte type)
        {
            if (_opcodes == null) _opcodes = LoadOpCodes();
            return _opcodes[type]?.Name;
        }

        private Dictionary<byte, OpCode> LoadOpCodes()
        {
            return GetResource<ResVocab>(998).GetVocabOpcodes();
        }

        private string[] _funcNames;

        public string GetFuncName(int ind)
        {
            if (_funcNames == null) _funcNames = LoadFuncs();
            if (ind >= _funcNames.Length) return $"kernel_{ind}";
            return _funcNames[ind];
        }

        private string[] LoadFuncs()
        {
            return GetResource<ResVocab>(999).GetText();
        }

        private string[] _names;

        public string[] Names => _names ??= GetResource<ResVocab>(997).GetVocabNames();

        public string GetName(int ind)
        {
            return Names[ind];
        }

        public IEnumerable<Resource> GetTextResources()
        {
            return GetResources(ResType.Text)
                .Union(GetResources(ResType.Script))
                .Union(GetResources(ResType.Message));
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
    }
}
