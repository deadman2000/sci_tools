using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Resources.Scripts1;
using System.Collections.Generic;
using System.Text;

namespace SCI_Lib.Resources.Scripts.Builders
{
    public class SimpeScriptBuilder : IScriptBuilder
    {
        readonly StringBuilder sb = new StringBuilder();
        Dictionary<ushort, string> _words;

        public string Decompile(Script script)
        {
            sb.Clear();
            _words = script.Package.GetIdToWord();

            foreach (Section sec in script.Sections)
                WriteSection(sec);

            return sb.ToString().TrimEnd();
        }

        public string Decompile(Script1 script)
        {
            sb.Clear();
            sb.AppendLine($"[Script {script.Resource.Number} - SCI 1.1]");
            sb.AppendLine("[Exports section]");
            for (int i = 0; i < script.Exports.Length; i++)
            {
                var export = script.Exports[i];
                sb.AppendLine($"\texport_{i} = {Script1Listing.ExportName(script, export)} ({export?.ToHex() ?? "0000"})");
            }
            sb.AppendLine();

            WriteCode(script.Operators);
            sb.AppendLine().AppendLine("[Heap pointers in script]");
            foreach (var pointer in script.HeapPointers)
            {
                if (pointer is BaseRef reference)
                    sb.AppendLine($"{reference.Address:x4}: pointer = {reference.TargetOffset:x4}");
            }

            sb.AppendLine().AppendLine("[Heap - Local variables]");
            for (int i = 0; i < script.Heap.LocalVars.Length; i++)
            {
                var local = script.Heap.LocalVars[i];
                sb.AppendLine($"{4 + i * 2:x4}: var[{i}] = {Script1Listing.Value(local.Value, local.StringValue)}");
            }

            foreach (var obj in script.Objects)
            {
                var super = obj.Super;
                sb.AppendLine().AppendLine($"{obj.Address:x4}: [Heap - {(obj.IsInstance ? "Object" : "Class")}]")
                    .AppendLine($"\tname = {Script1Listing.ObjectName(obj)}")
                    .AppendLine($"\tspecies = {obj.Id:x4}")
                    .AppendLine($"\tsuper = {super?.Name ?? $"${obj.Properties[6].Value:x4}"}");
                for (int i = 5; i < obj.Properties.Length; i++)
                {
                    var property = obj.Properties[i];
                    sb.AppendLine($"{obj.Address + i * 2:x4}: {Script1Listing.PropertyName(obj, super, i)} = {Script1Listing.Value(property.Value, property.StringValue)}");
                }
                foreach (var method in obj.Methods)
                    sb.AppendLine($"\tfunc[{method.Selector:x4}] = {method.Name} code_{method.Address:x4}");
            }

            sb.AppendLine().AppendLine("[Heap - String section]");
            foreach (var str in script.AllStrings())
                sb.AppendLine($"string_{str.Address:x4} = '{str.ValueSlashEsc}'");

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
                case SynonymSecion ss:
                    Write(ss);
                    break;
                case SaidSection ss:
                    Write(ss);
                    break;
                default:
                    sb.AppendLine($"[Section {sec.Type}]");
                    break;
            }
            sb.AppendLine();
        }

        private void Write(SaidSection ss)
        {
            sb.AppendLine($"[{ss.Type} section]");
            foreach (var said in ss.Saids)
                sb.AppendLine($"\t{said.Label}");
        }

        private string WordToStr(ushort word)
        {
            if (_words.TryGetValue(word, out var str)) return str;
            return $"{word:X03}";
        }

        private void Write(SynonymSecion ss)
        {
            sb.AppendLine($"[{ss.Type} section]");
            foreach (var syn in ss.Synonyms)
                sb.AppendLine($"\t{WordToStr(syn.WordA)} = {WordToStr(syn.WordB)}");
        }

        private void Write(ClassSection cs)
        {
            sb.AppendLine($"[{cs.Type} section]")
                .AppendLine($"\tname = {cs.Name}")
                .AppendLine($"\tspecies = {cs.Id:x}");

            for (int i = 0; i < cs.Properties.Length; i++)
            {
                var s = cs.Properties[i];
                sb.AppendLine($"{s.Address:x4}: sel[{i}] = {s}");
            }

            if (cs.Type == SectionType.Class)
            {
                sb.AppendLine();
                for (int i = 0; i < cs.Properties.Length; i++)
                    sb.AppendLine($"\tvarsel[{i}] = {cs.Properties[i].NameSel:x4}\t");
            }

            if (cs.FuncNamesInd.Length > 0)
            {
                sb.AppendLine();
                for (int i = 0; i < cs.FuncNamesInd.Length; i++)
                    sb.AppendLine($"\tfunc[{i}] = {cs.Package.GetName(cs.FuncNamesInd[i])} {cs.FuncCode[i]:x4}");
            }
        }

        private void Write(CodeSection cs)
        {
            WriteCode(cs.Operators);
        }

        private void WriteCode(IEnumerable<Code> operators)
        {
            sb.AppendLine($"[Code section]");
            foreach (var c in operators)
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
            short vi, vt;

            if ((c.Type & 1) == 0)
            {
                vt = c.GetShort(0);
                vi = c.GetShort(1);
            }
            else
            {
                vt = c.GetByte(0);
                vi = c.GetByte(1);
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
            {
                var r = rs.Refs[i];
                sb.AppendLine($"pointer_{r.Address:x4} = {r.ToHex()}; {r}");
            }
        }

        private void Write(StringSection ss)
        {
            sb.AppendLine($"[String section]");
            foreach (var s in ss.Strings)
            {
                sb.AppendLine($"string_{s.Address:x4} = '{s.ValueSlashEsc}'");
            }
        }
    }
}
