using SCI_Lib.Utils;

namespace SCI_Lib.Resources
{
    public class ResVocab997 : ResVocab
    {
        private string[] _vocabNames;

        public string[] GetVocabNames()
        {
            if (_vocabNames != null)
                return _vocabNames;

            var data = GetContent();

            ushort count = Helpers.GetUShortBE(data, 0);
            string[] names = new string[count];

            for (int i = 0; i < count; i++)
            {
                ushort addr = Helpers.GetUShortBE(data, i * 2 + 2);
                ushort len = Helpers.GetUShortBE(data, addr);
                string name = GameEncoding.GetStringEscape(data, addr + 2, len);
                names[i] = name;
            }

            return _vocabNames = names;
        }

    }
}
