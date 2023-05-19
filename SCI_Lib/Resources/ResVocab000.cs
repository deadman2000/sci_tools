using SCI_Lib.Resources.Vocab;
using SCI_Lib.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SCI_Lib.Resources
{
    public class ResVocab000 : ResVocab
    {
        private Word[] _words;

        public Word[] GetWords()
        {
            return _words ??= ReadWords();
        }

        private Word[] ReadWords()
        {
            var words = new List<Word>();
            var data = GetContent();
            var ms = new MemoryStream(data);

            byte[] currentWord = new byte[256];

            ms.Position = 26 * 2;

            while (true)
            {
                var pos = ms.ReadByte();
                if (pos < 0) break;

                while (true)
                {
                    var b = ms.ReadB();
                    currentWord[pos++] = (byte)(b & 0x7f);
                    if ((b & 0x80) > 0) break;
                }

                var id = ms.Read3ByteLE();
                string str = Encoding.ASCII.GetString(currentWord, 0, pos);
                words.Add(new Word(str, id));
            }

            return words.ToArray();
        }
    }
}
