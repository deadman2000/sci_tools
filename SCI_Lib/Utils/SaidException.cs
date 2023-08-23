using System;

namespace SCI_Lib.Utils;

public class SaidException : Exception
{
    public SaidException(string word, string message)
        : base(message)
    {
        Word = word;
    }

    public string Word { get; }
}
