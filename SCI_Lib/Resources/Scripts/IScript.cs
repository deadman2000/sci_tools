using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCI_Lib.Resources.Scripts
{
    public interface IScript
    {
        byte[] GetBytes();

        IEnumerable<StringConst> AllStrings();

        IClass GetClass(ushort id);


        List<T> Get<T>() where T : Section;
    }
}
