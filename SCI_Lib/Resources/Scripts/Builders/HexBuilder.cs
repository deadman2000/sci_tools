using SCI_Lib.Utils;
using System.Text;
using SCI_Lib.Resources.Scripts1;

namespace SCI_Lib.Resources.Scripts.Builders
{
    public class HexBuilder : IScriptBuilder
    {
        StringBuilder sb = new StringBuilder();

        private GameEncoding gameEncoding;

        public string Decompile(Script script)
        {
            sb.Clear();
            gameEncoding = script.Package.GameEncoding;

            foreach (var sec in script.Sections)
            {
                sb.Append(string.Format("Type: {0} Size: {1}\r\n", sec.Type, sec.Size));
                AddHex(script.SourceData, sec.Address, sec.Size);
                sb.Append("\r\n");
            }

            return sb.ToString().TrimEnd();
        }

        public string Decompile(Script1 script)
        {
            sb.Clear();
            gameEncoding = script.Package.GameEncoding;

            // Read the resource bytes without serializing and changing element addresses.
            var data = script.Resource.GetContent();
            sb.AppendLine($"Script {script.Resource.Number} Size: {data.Length}");
            AddHex(data, 0, data.Length);
            sb.AppendLine();

            var heap = script.Heap.Resource.GetContent();
            sb.AppendLine($"Heap {script.Resource.Number} Size: {heap.Length}");
            AddHex(heap, 0, heap.Length);

            return sb.ToString().TrimEnd();
        }

        void AddHex(byte[] data, int offset, int length)
        {
            int i = offset;
            int l;
            while (i < offset + length)
            {
                if (i + 16 > offset + length)
                    l = offset + length - i;
                else
                    l = 16;

                string str = string.Format("{0:X8}: {1,-48}  {2}\r\n", i, Helpers.ByteToHex(data, i, l), gameEncoding.PrintableString(data, i, l));
                sb.Append(str);
                i += 16;
            }
        }

    }
}
