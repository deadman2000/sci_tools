using SCI_Lib.Resources.Vocab;
using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCI_Lib.Resources
{
    public class ResVocab001 : ResVocab
    {
        private Word[] _words;

        public Word[] GetWords() => _words ??= ReadWords();

        public void SetWords(IEnumerable<Word> words)
        {
            _words = words.OrderBy(w => w.Text).ToArray();
        }

        private Word[] ReadWords()
        {
            var words = new List<Word>();
            var data = GetContent();
            var ms = new MemoryStream(data);

            byte[] currentWord = new byte[256];

            while (true)
            {
                var pos = ms.ReadByte();
                if (pos < 0) break;

                while (true)
                {
                    var b = ms.ReadB();
                    currentWord[pos++] = (byte)((b & 0x7f) | 0x80);
                    if ((b & 0x80) > 0) break;
                }

                var id = ms.Read3ByteLE();

                string str = Package.GameEncoding.GetString(currentWord, 0, pos);
                words.Add(new Word(str, id));
            }

            return words.ToArray();
        }

        public override byte[] GetPatch()
        {
            ByteBuilder bb = new ByteBuilder();

            byte[] currentWord = new byte[256];

            foreach (var word in _words)
            {
                var bytes = Package.GameEncoding.GetBytes(word.Text);
                bool isStart = true;
                byte pos = 0;
                for (int i = 0; i < bytes.Length; i++)
                {
                    var c = bytes[i];
                    if (c < 0x80) throw new FormatException($"Invalid symbol in translated word {word.Text}");

                    c &= 0x7f;
                    if (isStart)
                    {
                        if (currentWord[i] == c)
                        {
                            pos++;
                            continue;
                        }
                        else
                        {
                            bb.AddByte(pos);
                            isStart = false;
                        }
                    }
                    currentWord[i] = c;

                    if (i == bytes.Length - 1) c |= 0x80;
                    bb.AddByte(c);
                }

                if (isStart)
                {
                    pos--;
                    var last = currentWord[pos];
                    bb.AddByte(pos);
                    bb.AddByte((byte)(last | 0x80));
                }

                currentWord[word.Text.Length] = 0;

                bb.AddThreeBytesLE(word.Id);
            }

            return bb.GetArray();
        }
    }
}
