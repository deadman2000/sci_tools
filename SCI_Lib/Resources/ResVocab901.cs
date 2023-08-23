using SCI_Lib.Resources.Vocab;
using SCI_Lib.Utils;
using System.Collections.Generic;
using System.IO;

namespace SCI_Lib.Resources
{
    public class ResVocab901 : ResVocab
    {
        private Suffix[] _suffixes;

        public Suffix[] GetSuffixes() => _suffixes ??= ReadSuffixes();

        public void SetSuffixes(Suffix[] suffixes)
        {
            _suffixes = suffixes;
        }

        private Suffix[] ReadSuffixes()
        {
            var suffixes = new List<Suffix>();
            var data = GetContent();
            var ms = new MemoryStream(data);

            while (!ms.IsEnd() && ms.Peek() != 0xff)
            {
                var output = ms.ReadString(Package.GameEncoding);
                var suffixClass = ms.ReadUShortLE();
                if (suffixClass == 0xffff) break;
                var pattern = ms.ReadString(Package.GameEncoding);
                var inputClass = ms.ReadUShortLE();
                suffixes.Add(new Suffix(output.TrimStart('*'), suffixClass, pattern.TrimStart('*'), inputClass));
            }

            return suffixes.ToArray();
        }

        static byte[] END = new byte[] { 0, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };

        public override byte[] GetPatch()
        {
            ByteBuilder bb = new ByteBuilder();

            foreach (var s in _suffixes)
            {
                bb.AddString("*" + s.Output, Package.GameEncoding);
                bb.AddByte(0);
                bb.AddUShortLE((ushort)s.SuffixClass);
                bb.AddString("*" + s.Pattern, Package.GameEncoding);
                bb.AddByte(0);
                bb.AddUShortLE((ushort)s.InputClass);
            }
            bb.AddBytes(END);

            return bb.GetArray();
        }
    }
}
