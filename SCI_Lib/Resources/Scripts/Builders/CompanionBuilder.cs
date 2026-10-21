using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Resources.Scripts1;
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
            sb.Clear();
            _methods.Clear();
            _words = script.Package.GetIdToWord();

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

        public string Decompile(Script1 script)
        {
            sb.Clear();
            _methods.Clear();
            sb.AppendLine($"(script {script.Resource.Number})").AppendLine();

            sb.AppendLine("(exports");
            for (int i = 0; i < script.Exports.Length; i++)
                sb.AppendLine($"    {i} {Script1Listing.ExportName(script, script.Exports[i])}");
            sb.AppendLine(")").AppendLine();

            sb.AppendLine("(string");
            foreach (var str in script.AllStrings())
                sb.AppendLine($"    string_{str.Address:x4} \"{str.ValueSlashEsc}\"");
            sb.AppendLine(")").AppendLine();

            sb.AppendLine("(local");
            for (int i = 0; i < script.Heap.LocalVars.Length; i++)
            {
                var local = script.Heap.LocalVars[i];
                sb.AppendLine($"    local{i} = {Script1Listing.Value(local.Value, local.StringValue)}");
            }
            sb.AppendLine(")").AppendLine();

            foreach (var method in script.Objects.SelectMany(obj => obj.Methods))
                _methods.Add(method.Address);

            // SCI 1.1 has one code block. Bound each body by the next entry point,
            // rather than stopping at a ret or following an address in the heap.
            var entries = new SortedSet<ushort>(_methods);
            foreach (var export in script.Exports)
                if (export != null && !Script1Listing.IsHeapExport(script, export))
                    entries.Add(export.TargetOffset);
            foreach (var code in script.Operators)
            {
                if (code.Name == "call")
                    foreach (var reference in code.Arguments.OfType<BaseRef>())
                        entries.Add(reference.TargetOffset);
            }
            if (script.Operators.Count > 0)
                entries.Add(script.Operators[0].Address);

            var starts = entries.Where(address => script.GetOperator(address) != null).ToArray();
            var ends = new Dictionary<ushort, int>();
            for (int i = 0; i < starts.Length; i++)
                ends[starts[i]] = i + 1 < starts.Length ? starts[i + 1] : int.MaxValue;

            foreach (var address in starts.Where(address => !_methods.Contains(address)))
            {
                sb.AppendLine($"(procedure (localproc_{address:x4})");
                WriteCode(script.GetOperator(address), ends[address], false);
                sb.AppendLine(")").AppendLine();
            }

            foreach (var obj in script.Objects)
            {
                var super = obj.Super;
                sb.AppendLine($"// Heap {obj.Address:x4}");
                sb.Append($"({(obj.IsInstance ? "instance" : "class")} {Script1Listing.ObjectName(obj)}");
                if (super != null)
                    sb.Append($" of {Script1Listing.ObjectName(super)}");
                sb.AppendLine().AppendLine("    (properties");
                for (int i = 8; i < obj.Properties.Length; i++)
                {
                    var property = obj.Properties[i];
                    sb.AppendLine($"        {Script1Listing.PropertyName(obj, super, i)} = {Script1Listing.Value(property.Value, property.StringValue)}");
                }
                sb.AppendLine("    )");

                foreach (var method in obj.Methods)
                {
                    sb.AppendLine($"    (method ({method.Name ?? $"selector_{method.Selector:x4}"}) // method_{method.Address:x4}");
                    if (ends.TryGetValue(method.Address, out var end))
                        WriteCode(script.GetOperator(method.Address), end, false);
                    sb.AppendLine("    )").AppendLine();
                }
                sb.AppendLine(")").AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private void WriteProc(CodeSection cs)
        {
            if (_methods.Contains((ushort)cs.Address)) return;

            var code = cs.Operators.FirstOrDefault();

            while (code != null && code.Type != 0)
            {
                sb.AppendFormat("(procedure (localproc_{0:x4})", code.Address).AppendLine();
                code = WriteCode(code);
                sb.AppendLine(")");
                sb.AppendLine();
            }
        }

        private Code WriteCode(Code code, int endAddress = int.MaxValue, bool stopAtReturn = true)
        {
            ushort lookTo = 0; // Адрес переходов
            while (code != null && code.Address < endAddress)
            {
                if (code.XRefs.Any(r => r is not GlobalRef))
                    sb.AppendLine($"        {code.Label}");

                string args;
                if (code.Name == "pushi")
                    args = ArgsDecToString(code);
                else
                    args = ArgsToString(code);

                foreach (var arg in code.Arguments)
                    if (arg is BaseRef r)
                        lookTo = Math.Max(lookTo, r.TargetOffset);

                sb.AppendLine($"  {code.Address:x4}:{code.Type:x2} {ArgsHexToString(code),-13} {code.Name,5} {args}");

                if (stopAtReturn && code.IsReturn && code.Address >= lookTo)
                    break;

                if (code.IsJump)
                    sb.AppendLine();

                code = code.Next;
            }

            return code?.Next;
        }

        private void WriteStrings(StringSection section)
        {
            sb.AppendLine("(string");
            if (section != null)
            {
                foreach (var str in section.Strings)
                    sb.AppendFormat("    string_{0:x4} \"{1}\"", str.Address, str.ValueSlashEsc).AppendLine();
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
            s.Prepare();
            sb.AppendFormat("// {0:x4}", s.Address + 2).AppendLine();
            var super = s.SuperClass;
            sb.AppendFormat("({0} {1} of {2}",
                s.Type == SectionType.Class ? "class" : "instance",
                s.Name,
                super != null ? super.Name : "").AppendLine();

            sb.AppendLine("    (properties");
            var pack = s.Package;
            for (int i = 4; i < s.Properties.Length; i++)
            {
                var prop = s.Properties[i];
                if (i < s.PropNamesInd.Length)
                    sb.AppendFormat("        {0} = {1}", prop.Name, prop.ValueStr).AppendLine();
                else
                    sb.AppendFormat("        !!out of range!! {0:x}", prop).AppendLine();
            }
            sb.AppendLine("    )");

            for (int i = 0; i < s.FuncNamesInd.Length; i++)
            {
                var addr = s.FuncCode[i].TargetOffset;

                sb.AppendLine($"    (method ({pack.GetName(s.FuncNamesInd[i])}) // method_{addr:x4}");

                var code = s.Script.GetOperator(addr);
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
                    case BaseRef r:
                        sb.Append($"{r.ToHex():x4}");
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
                    case BaseRef r:
                        if (r.Reference != null)
                            sb.Append(r.Reference.Label);
                        else
                            sb.Append($"ref_{r.TargetOffset:x4}");
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
                    case BaseRef r:
                        if (r.Reference != null)
                            sb.Append(r.Reference.Label);
                        else
                            sb.Append($"ref_{r.TargetOffset:x4}");
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
