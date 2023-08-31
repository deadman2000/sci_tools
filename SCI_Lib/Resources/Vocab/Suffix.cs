using System.Linq;

namespace SCI_Lib.Resources.Vocab
{
    public class Suffix
    {
        public Suffix(string output, ushort suffixClass, string pattern, ushort inputClass)
        {
            Output = output;
            SuffixClass = (WordClass)suffixClass;
            Pattern = pattern;
            InputClass = (WordClass)inputClass;
        }

        public string Pattern { get; set; }
        public WordClass InputClass { get; set; }
        public string InputClassHex => $"{(ushort)InputClass:X03}";
        public string InputClassStr
        {
            get => $"{InputClass} [{InputClassHex}]";
            set
            {
                if (ushort.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out var res))
                    InputClass = (WordClass)res;
            }
        }

        public string Output { get; set; }
        public WordClass SuffixClass { get; set; }
        public string SuffixClassHex => $"{(ushort)SuffixClass:X03}";
        public string SuffixClassStr
        {
            get => $"{SuffixClass} [{SuffixClassHex}]";
            set
            {
                if (ushort.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out var res))
                    SuffixClass = (WordClass)res;
            }
        }

        public override string ToString()
        {
            return $"{Pattern} [{InputClass}] => {Output} [{SuffixClass}]";
        }

        public bool IsEn => (Pattern.Length == 0 || Pattern.All(c => c <= 'z')) &&
            (Output.Length == 0 || Output.All(c => c <= 'z'));


        /// <summary>
        /// Определяет, является ли входное слово результатом применения суффикса и возвращает исходное слово
        /// </summary>
        /// <param name="word"></param>
        /// <param name="newWord"></param>
        /// <returns></returns>
        public bool IsMatchReverse(string word, out string newWord)
        {
            if (Output.Length == 0 || word.EndsWith(Output))
            {
                newWord = word[..^Output.Length] + Pattern;
                return true;
            }

            newWord = null;
            return false;
        }

        /// <summary>
        /// Определяет, подходит ли слово под данный суффикс
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public bool IsMatch(Word word)
        {
            if ((InputClass & word.Class) != word.Class) return false;
            return Pattern.Length == 0 || word.Text.EndsWith(Pattern);
        }

        /// <summary>
        /// Определяет, подходит ли слово под данный суффикс
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public bool IsMatch(Word word, out string newWord)
        {
            if ((word.IsEn != IsEn) ||
                ((InputClass & word.Class) != word.Class))
            {
                newWord = null;
                return false;
            }

            if (Pattern.Length == 0 || word.Text.EndsWith(Pattern))
            {
                newWord = word.Text[..^Pattern.Length] + Output;
                return true;
            }

            newWord = null;
            return false;
        }
    }
}
