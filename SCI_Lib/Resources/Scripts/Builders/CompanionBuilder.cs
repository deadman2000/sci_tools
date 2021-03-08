using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Scripts.Elements;
using System.Linq;
using System.Text;

namespace SCI_Lib.Resources.Scripts.Builders
{
    public class CompanionBuilder : IScriptBuilder
    {
        StringBuilder sb = new StringBuilder();

        public string Decompile(Script script)
        {
            sb.AppendFormat("(script {0})", script.Resource.Number).AppendLine();
            sb.AppendLine();

            script.Get<StringSection>().ForEach(s => WriteStrings(s));
            script.Get<PreloadTextSection>().ForEach(s => sb.AppendLine("(said").AppendLine(")").AppendLine());
            script.Get<LocalVariablesSection>().ForEach(s => WriteLocals(s));
            script.Get<ClassSection>(SectionType.Class).ForEach(s => WriteClass(s));
            script.Get<ObjectSection>().ForEach(s => WriteClass(s));

            return sb.ToString().TrimEnd();
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

                    sb.Append($"    local{i} = ${locals.Vars[i]:x4}").AppendLine();
                }
            }
            sb.AppendLine(")");
            sb.AppendLine();
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
                sb.AppendLine($"    (method ({pack.GetName(s.FuncNames[i])}) // method_{s.FuncCode[i].TargetOffset:x4}");

                Code code = s.Script.GetElement(s.FuncCode[i].TargetOffset) as Code;
                while (code != null)
                {
                    if (code.XRefs.Any(r => !(r is FuncRef)))
                        sb.AppendLine($"        {code.Label}");

                    sb.AppendLine($"  {code.Address:x4}:{code.Type:x2} {ArgsHexToString(code),-13} {code.Name,5} {ArgsToString(code)}");

                    if (code.IsReturn)
                        break;

                    if (code.IsCall)
                        sb.AppendLine();

                    code = code.Next;
                }

                sb.AppendLine("    )");
            }

            sb.AppendLine(")");
            sb.AppendLine();
        }

        private string ArgsHexToString(Code code)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < code.Arguments.Count; i++)
            {
                object a = code.Arguments[i];
                if (a is byte)
                    sb.Append($"{a:x2}");
                else if (a is ushort)
                    sb.Append($"{a:x4}");
                else if (a is RefToElement r)
                    sb.Append($"{r.Value:x4}");
                else if (a is LinkToExport l)
                    sb.AppendFormat("{0:x4} {1:x4}", l.ScriptNumber, l.ExportNumber);
                else
                    sb.Append(a.ToString());
                sb.Append(" ");
            }
            return sb.ToString().Trim();
        }

        private string ArgsToString(Code code)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < code.Arguments.Count; i++)
            {
                object a = code.Arguments[i];
                if (a is byte)
                    sb.Append($"{a:x}");
                else if (a is ushort)
                    sb.Append($"{a:x}");
                else if (a is RefToElement r)
                {
                    if (r.Reference != null)
                        sb.Append(r.Reference.Label);
                    else
                        sb.Append($"ref_{r.TargetOffset:x4}");
                }
                else if (a is LinkToExport l)
                    sb.AppendFormat("{0:x} procedure_{1:x4}", l.ScriptNumber, l.ExportNumber);
                else
                    sb.Append(a.ToString());
                sb.Append(" ");
            }
            return sb.ToString().Trim();
        }
    }
}
