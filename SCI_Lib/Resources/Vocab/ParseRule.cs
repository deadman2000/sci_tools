using System.Collections.Generic;

namespace SCI_Lib.Resources.Vocab;

internal class ParseRule
{
    public ParseRule()
    {
    }

    public ParseRule(ParseRule copy)
    {
        Id = copy.Id;
        FirstSpecial = copy.FirstSpecial;
        NumSpecials = copy.NumSpecials;
        Data = new(copy.Data);
    }

    public int Id { get; set; }
    public int FirstSpecial { get; set; }
    public int NumSpecials { get; set; }
    public List<uint> Data { get; set; }

    public ParseRule Clone() => new ParseRule(this);
}