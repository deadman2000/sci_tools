using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System.Collections.Generic;

namespace SCI_Lib.Resources.Scripts
{
    public interface IScript
    {
        byte[] GetBytes();

        IEnumerable<StringConst> AllStrings();

        IEnumerable<T> Get<T>() where T : Section;
    }
}
