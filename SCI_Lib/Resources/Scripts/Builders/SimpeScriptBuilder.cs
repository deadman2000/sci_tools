using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System.Text;

namespace SCI_Lib.Resources.Scripts.Builders
{
    public class SimpeScriptBuilder : IScriptBuilder
    {
        readonly StringBuilder sb = new StringBuilder();

        public string Decompile(Script script)
        {
            foreach (Section sec in script.Sections)
                WriteSection(sec);

            return sb.ToString().TrimEnd();
        }

        private void WriteSection(Section sec)
        {
            sb.Append($"{sec.Address:x4}: ");

            switch (sec)
            {
                case ClassSection cs:
                    Write(cs);
                    break;
                case CodeSection cs:
                    Write(cs);
                    break;
                case ExportSection es:
                    Write(es);
                    break;
                case LocalVariablesSection lvs:
                    Write(lvs);
                    break;
                case PreloadTextSection pts:
                    Write(pts);
                    break;
                case RelocationSection rs:
                    Write(rs);
                    break;
                case StringSection ss:
                    Write(ss);
                    break;
                default:
                    sb.AppendLine($"[Section {sec.Type}]");
                    break;
            }
            sb.AppendLine();
        }

        private void Write(ClassSection cs)
        {
            sb.AppendLine($"[{cs.Type} section]")
                .AppendLine($"\tname = {cs.Name}")
                .AppendLine($"\tspecies = {cs.Id:x}");

            for (int i = 0; i < cs.Selectors.Length; i++)
            {
                var s = cs.Selectors[i];
                sb.AppendLine($"{s.Address:x4}: sel[{i}] = {s}");
            }

            if (cs.Type == SectionType.Class)
            {
                sb.AppendLine();
                for (int i = 0; i < cs.Varselectors.Length; i++)
                    sb.AppendLine($"\tvarsel[{i}] = {cs.Varselectors[i]:x4}\t");
            }

            if (cs.FuncNames.Length > 0)
            {
                sb.AppendLine();
                for (int i = 0; i < cs.FuncNames.Length; i++)
                    sb.AppendLine($"\tfunc[{i}] = {cs.Package.GetName(cs.FuncNames[i])} {cs.FuncCode[i]:x4}");
            }
        }

        private void Write(CodeSection cs)
        {
            sb.AppendLine($"[Code section]");
            foreach (var c in cs.Operators)
            {
                if (c.XRefs.Count > 0)
                    sb.AppendLine().AppendLine($"{c.Label}:");

                sb.Append($"{c.Address:x4}: {c.Type:x2} {c.ArgsToHex(),-8}  {c.Name,-8} {c.ArgsToString()}");

                if (c.Name.Equals("lea"))
                {
                    sb.Append(";  ");
                    FillLeaDescription(c, sb);
                }

                sb.AppendLine();
            }
        }

        private void FillLeaDescription(Code c, StringBuilder sb)
        {
            ushort vi, vt;

            if ((c.Type & 1) == 0)
            {
                vt = (ushort)c.Arguments[0];
                vi = (ushort)c.Arguments[1];
            }
            else
            {
                vt = (byte)c.Arguments[0];
                vi = (byte)c.Arguments[1];
            }

            sb.Append(LeaVarType(vt >> 1 & 3))
                .Append("[");

            if ((vt & 0x10) != 0)
                sb.Append("acc+");

            if ((c.Type & 1) == 0)
                sb.Append(vi.ToString("x4"));
            else
                sb.Append(vi.ToString("x2"));

            sb.Append("]");
        }

        private string LeaVarType(int t)
        {
            switch (t)
            {
                case 0: return "glob";
                case 1: return "var";
                case 2: return "temp";
                default: return "parm";
            }
        }

        private void Write(ExportSection es)
        {
            sb.AppendLine("[Exports section]");
            for (int i = 0; i < es.Exports.Length; i++)
            {
                sb.AppendLine($"\texport_{i} = {es.Exports[i]}");
            }
        }

        private void Write(LocalVariablesSection lvs)
        {
            sb.AppendLine($"[Local variables]");
            for (int i = 0; i < lvs.Vars.Length; i++)
            {
                //sb.AppendLine($"var[{i}] = {lvs.Refs[i]?.ToString() ?? "0"}");
                sb.AppendLine($"var[{i}] = ${lvs.Vars[i]:x}");
            }
        }

        private void Write(PreloadTextSection _)
        {
            sb.AppendLine($"[Preload section]");
        }

        private void Write(RelocationSection rs)
        {
            sb.AppendLine($"[Relocation section]");
            for (int i = 0; i < rs.Refs.Length; i++)
                sb.AppendLine($"\tpointer[{i}] = {rs.Refs[i].Value:x4}; {rs.Refs[i]}");
        }

        private void Write(StringSection ss)
        {
            sb.AppendLine($"[String section]");
            foreach (var s in ss.Strings)
            {
                sb.AppendLine($"{s.Address:x4} = '{s.GetStringEscape()}'");
            }
        }
    }
}
