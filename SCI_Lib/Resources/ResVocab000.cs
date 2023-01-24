using SCI_Lib.Resources.Vocab;
using System.Collections.Generic;

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


            return words.ToArray();
        }
    }
}
