using System;

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
    }
}
