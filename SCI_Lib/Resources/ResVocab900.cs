using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCI_Lib.Resources
{
    public class ResVocab900 : ResVocab
    {
        public void ReadBranches()
        {
            var data = GetContent();
            var ms = new MemoryStream(data);

            int branchesCount = data.Length / 20;

            for (int i = 0; i < branchesCount; i++)
            {
                var id = ms.ReadUShortBE();

                ushort[] values = new ushort[9];
                for (int k = 0; k < 9; k++)
                    values[k] = ms.ReadUShortBE();

                var valsStr = string.Join(", ", values.Select(v => v.ToString("X3")));
                Console.WriteLine($"{id:X3} {valsStr}");
            }
        }
    }
}
