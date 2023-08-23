using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Resources.Vocab;

public class LookupResult
{
    public string Word { get; set; }
    public IEnumerable<ParsedWord> Ids { get; set; }
    public bool IsValid => Ids != null;

    public override string ToString()
    {
        if (Ids == null) return $"{Word} unknown";

        var idsStr = string.Join(", ", Ids.Select(i => i.ToString()));
        return $"{Word} => {idsStr}";
    }
}
