using System.Collections.Generic;

namespace SCI_Lib.Resources
{
    public class ResVocab999 : ResVocab
    {
        private string[] _text;
      
        /// <summary>
        /// \0 terminated lines
        /// </summary>
        /// <returns></returns>
        public string[] GetText()
        {
            return _text ??= ReadText();
        }

        private string[] ReadText()
        {
            List<string> lines = new List<string>();
            var data = GetContent();

            int s = 0;
            int ind = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0x00)
                {
                    string strRes = GameEncoding.GetString(data, s, i - s);
                    lines.Add(strRes);
                    ind++;
                    s = i + 1;
                }
            }

            return lines.ToArray();
        }
    }
}
