using SCI_Lib.Resources.Scripts.Analyzer;
using SCI_Lib.Resources.Scripts.Sections;
using System.Linq;
using System.Text;

namespace SCI_Lib.Resources.Scripts.Builders;

public class CppBuilder : IScriptBuilder
{
    private readonly StringBuilder sb = new();
    private int _level = 0;
    private ScriptAnalyzer _analyzer;

    public string Decompile(Script script)
    {
        _analyzer = script.Analyze();

        sb.AppendLine("#pragma once")
            .AppendLine()
            .AppendLine("#include \"global.h\"")
            .AppendLine();

        foreach (var s in script.Get<ClassSection>(SectionType.Class)) WriteClass(s);
        foreach (var s in script.Get<ObjectSection>()) WriteClass(s);

        return sb.ToString();
    }

    private void WriteClass(ClassSection s)
    {
        s.Prepare();
        var name = s.Name;
        if (s.Type == SectionType.Object) name += "Class";

        var super = s.SuperClass;

        sb.Append($"class {name}");
        if (super != null)
            sb.Append($" : {super.Name}");
        sb.AppendLine()
            .AppendLine("{");
        _level++;

        var pack = s.Package;
        for (int i = 4; i < s.Properties.Length; i++)
        {
            var prop = s.Properties[i];
            Space().Append($"short {prop.Name} = {prop.Value};");
            if (prop.Value > 9) sb.Append($" // 0x{prop.Value:x4}");
            sb.AppendLine();
        }
        sb.AppendLine();

        for (int i = 0; i < s.FuncNamesInd.Length; i++)
        {
            var addr = s.FuncCode[i].TargetOffset;
            var method = pack.GetName(s.FuncNamesInd[i]);

            var proc = _analyzer.Procedures.FirstOrDefault(p => p.Class == s && p.Name == method);

            AppendLine($"void {method}()");
            AppendLine("{");
            _level++;

            _level--;
            AppendLine("}");
            sb.AppendLine();
        }

        _level--;
        sb.Append('}');
        if (s.Type == SectionType.Object)
            sb.Append($" {s.Name}");
        sb.AppendLine(";");
    }

    private StringBuilder Space()
    {
        for (int i = 0; i < _level; i++)
            sb.Append("    ");
        return sb;
    }

    private void AppendLine(string line) => Space().AppendLine(line);

}
