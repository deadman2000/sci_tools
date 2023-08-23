using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Resources.Vocab;

public class TokenizeResult
{
    public List<LookupResult> Words { get; } = new();

    public bool IsValid => Words.All(w => w.IsValid);
}