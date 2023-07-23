using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Scripts.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCI_Lib.Resources.Scripts.Builders
{
    public class CompanionBuilder : IScriptBuilder
    {
        readonly StringBuilder sb = new();
        private Dictionary<ushort, string> _words;
        private HashSet<ushort> _methods = new();

        public string Decompile(Script script)
        {
            _words = script.Package.GetWords();

            sb.AppendFormat("(script {0})", script.Resource.Number).AppendLine();
            sb.AppendLine();

            var methods = script.Get<ClassSection>(SectionType.Class)
                .Union(script.Get<ObjectSection>())
                .SelectMany(s => s.FuncCode)
                .Select(f => f.TargetOffset)
                .Distinct();
            foreach (var m in methods) _methods.Add(m);

            foreach (var s in script.Get<StringSection>()) WriteStrings(s);
            foreach (var s in script.Get<SaidSection>()) WriteSaid(s);
            foreach (var s in script.Get<SynonymSecion>()) WriteSynonym(s);
            foreach (var s in script.Get<LocalVariablesSection>()) WriteLocals(s);
            foreach (var s in script.Get<CodeSection>()) WriteProc(s);
            foreach (var s in script.Get<ClassSection>(SectionType.Class)) WriteClass(s);
            foreach (var s in script.Get<ObjectSection>()) WriteClass(s);

            return sb.ToString().TrimEnd();
        }

        private void WriteProc(CodeSection cs)
        {
            if (_methods.Contains((ushort)cs.Address)) return;
            sb.AppendFormat("(procedure (localproc_{0:x4})", cs.Address).AppendLine();
            
            var code = cs.Operators.FirstOrDefault();
            WriteCode(code);
            
            sb.AppendLine(")");
            sb.AppendLine();
        }

        private void WriteCode(Code code)
        {
            ushort lookTo = 0; // Адрес переходов
            while (code != null)
            {
                if (code.XRefs.Any(r => r is not FuncRef))
                    sb.AppendLine($"        {code.Label}");

                string args;
                if (code.Name == "pushi")
                    args = ArgsDecToString(code);
                else
                    args = ArgsToString(code);

                foreach (var arg in code.Arguments)
                    if (arg is CodeRef cr)
                        lookTo = Math.Max(lookTo, cr.TargetOffset);

                sb.AppendLine($"  {code.Address:x4}:{code.Type:x2} {ArgsHexToString(code),-13} {code.Name,5} {args}");

                if (code.IsReturn && code.Address >= lookTo)
                    break;

                if (code.IsCall)
                    sb.AppendLine();

                code = code.Next;
            }
        }

        private void WriteStrings(StringSection section)
        {
            sb.AppendLine("(string");
            if (section != null)
            {
                foreach (var str in section.Strings)
                    sb.AppendFormat("    string_{0:x4} \"{1}\"", str.Address, str.Value).AppendLine();
            }
            sb.AppendLine(")");
            sb.AppendLine();
        }

        private void WriteSaid(SaidSection ss)
        {
            sb.AppendLine("(said");
            for (int i = 0; i < ss.Saids.Count; i++)
            {
                SaidExpression said = ss.Saids[i];
                sb.AppendLine($"    said[{i}] {said.Address:x4} {said.Label}");
            }

            sb.AppendLine(")");
            sb.AppendLine();
        }

        private string WordToStr(ushort word)
        {
            if (_words.TryGetValue(word, out var str)) return str;
            return $"{word:X03}";
        }

        private void WriteSynonym(SynonymSecion ss)
        {
            sb.AppendLine("(synonym");
            foreach (var syn in ss.Synonyms)
                sb.AppendLine($"    {WordToStr(syn.WordA)} = {WordToStr(syn.WordB)}");
            sb.AppendLine(")");
            sb.AppendLine();
        }

        private void WriteLocals(LocalVariablesSection locals)
        {
            sb.AppendLine("(local");
            if (locals != null)
            {
                for (int i = 0; i < locals.Vars.Length; i++)
                {
                    //var offset = locals.Refs[i]?.TargetOffset ?? 0;
                    //sb.Append($"    local{i} = ${offset:x4}" ).AppendLine();

                    sb.AppendLine($"    local{i} = ${locals.Vars[i]:x4}");
                }
            }
            sb.AppendLine(")");
            sb.AppendLine();
        }

        private void WriteClass(ClassSection s)
        {
            sb.AppendFormat("// {0:x4}", s.Address + 2).AppendLine();
            var super = s.SuperClass;
            sb.AppendFormat("({0} {1} of {2}",
                s.Type == SectionType.Class ? "class" : "instance",
                s.Name,
                super != null ? super.Name : "").AppendLine();

            sb.AppendLine("    (properties");
            var pack = s.Package;
            for (int i = 4; i < s.Selectors.Length; i++)
            {
                if (i < s.Varselectors.Length)
                    sb.AppendFormat("        {0} {1:x}", pack.GetName(s.Varselectors[i]), s.Selectors[i]).AppendLine();
                else
                    sb.AppendFormat("        !!out of range!! {0:x}", s.Selectors[i]).AppendLine();
            }
            sb.AppendLine("    )");

            for (int i = 0; i < s.FuncNames.Length; i++)
            {
                var addr = s.FuncCode[i].TargetOffset;

                sb.AppendLine($"    (method ({pack.GetName(s.FuncNames[i])}) // method_{addr:x4}");

                Code code = s.Script.GetElement(addr) as Code;
                WriteCode(code);

                sb.AppendLine("    )");
                sb.AppendLine();
            }

            sb.AppendLine(")");
            sb.AppendLine();
        }

        private static string ArgsHexToString(Code code)
        {
            StringBuilder sb = new();
            foreach (object a in code.Arguments)
            {
                switch (a)
                {
                    case byte _:
                        sb.Append($"{a:x2}");
                        break;
                    case ushort _:
                        sb.Append($"{a:x4}");
                        break;
                    case RefToElement r:
                        sb.Append($"{r.Value:x4}");
                        break;
                    case LinkToExport l:
                        sb.AppendFormat("{0:x4} {1:x4}", l.ScriptNumber, l.ExportNumber);
                        break;
                    default:
                        sb.Append(a.ToString());
                        break;
                }
                sb.Append(' ');
            }
            return sb.ToString().Trim();
        }

        private static string ArgsToString(Code code)
        {
            StringBuilder sb = new();
            foreach (object a in code.Arguments)
            {
                switch (a)
                {
                    case byte:
                        sb.Append($"{a:x}");
                        break;
                    case ushort:
                        sb.Append($"{a:x}");
                        break;
                    case RefToElement r:
                        if (r.Reference != null)
                            sb.Append(r.Reference.Label);
                        else
                            sb.Append($"ref_{r.TargetOffset:x4}");
                        break;
                    case LinkToExport l:
                        sb.AppendFormat("{0:x} procedure_{1:x4}", l.ScriptNumber, l.ExportNumber);
                        break;
                    default:
                        sb.Append(a.ToString());
                        break;
                }
                sb.Append(' ');
            }
            return sb.ToString().Trim();
        }

        private static string ArgsDecToString(Code code)
        {
            StringBuilder sb = new();
            foreach (object a in code.Arguments)
            {
                switch (a)
                {
                    case byte b:
                        sb.Append(b);
                        break;
                    case ushort s:
                        sb.Append(s);
                        break;
                    case RefToElement r:
                        if (r.Reference != null)
                            sb.Append(r.Reference.Label);
                        else
                            sb.Append($"ref_{r.TargetOffset:x4}");
                        break;
                    case LinkToExport l:
                        sb.AppendFormat("{0:x} procedure_{1:x4}", l.ScriptNumber, l.ExportNumber);
                        break;
                    default:
                        sb.Append(a.ToString());
                        break;
                }
                sb.Append(' ');
            }
            return sb.ToString().Trim();
        }

    }
}
