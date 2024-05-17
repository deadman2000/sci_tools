using SCI_Lib.Resources.Scripts.Elements;
using System.Collections.Generic;

namespace SCI_Lib.Resources.Scripts
{
    public interface ICodeBlock
    {
        List<Code> Operators { get; }

        BaseScript CodeOwner { get; }
    }
}
