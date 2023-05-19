using System;
using System.Collections.Generic;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public class SaidData
    {
        public ushort Data { get; set; }

        public bool IsOperator { get; set; }

        public SaidData(byte val)
        {
            Data = val;
            IsOperator = true;
        }

        public SaidData(char letter)
        {
            Data = LetterToOp(letter);
            IsOperator = true;
        }

        public SaidData(ushort word)
        {
            Data = word;
            IsOperator = false;
        }

        //public string Hex => IsOperator ? $"{Data:x02}" : $"{Data:x03}";
        public string Hex => IsOperator ? Letter : $"{Data:x03}";

        public string Letter => Data switch
        {
            0xf0 => ",",
            0xf1 => "&",
            0xf2 => "/",
            0xf3 => "(",
            0xf4 => ")",
            0xf5 => "[",
            0xf6 => "]",
            0xf7 => "#",
            0xf8 => "<",
            0xf9 => ">",
            _ => $"{{{Data:X02}}}",
        };

        public string ToString(Dictionary<ushort, string> words)
        {
            if (IsOperator)
                return Letter;

            if (words.TryGetValue(Data, out var word)) return word;
            //if (words.TryGetValue(Data, out var word)) return $"{word}{{{Data:x03}}}";
            return $"{Data:X03}";
        }

        private static ushort LetterToOp(char letter)
        {
            return letter switch
            {
                ',' => 0xf0,
                '&' => 0xf1,
                '/' => 0xf2,
                '(' => 0xf3,
                ')' => 0xf4,
                '[' => 0xf5,
                ']' => 0xf6,
                '#' => 0xf7,
                '<' => 0xf8,
                '>' => 0xf9,
                _ => throw new Exception($"Unknown said letter '{letter}'")
            };
        }
    }
}
