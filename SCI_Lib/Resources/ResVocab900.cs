using SCI_Lib.Resources.Vocab;
using SCI_Lib.Utils;
using System.Collections.Generic;
using System.IO;

namespace SCI_Lib.Resources
{
    public class ResVocab900 : ResVocab
    {
        private List<ParseTreeBranch> _branches;

        public List<ParseTreeBranch> GetBranches() => _branches ??= ReadBranches();

        private List<ParseTreeBranch> ReadBranches()
        {
            var data = GetContent();
            var ms = new MemoryStream(data);

            int branchesCount = data.Length / 20;
            List<ParseTreeBranch> list = new(branchesCount);

            for (int i = 0; i < branchesCount; i++)
            {
                var br = new ParseTreeBranch
                {
                    Id = ms.ReadUShortBE()
                };

                List<ushort> values = new(9);
                for (int k = 0; k < 9; k++)
                {
                    var v = ms.ReadUShortBE();
                    if (v != 0)
                        values.Add(v);
                }
                br.Data = values;

                if (br.Id != 0)
                    list.Add(br);
            }

            return list;
        }
    }
}
