using SCI_Lib.Analyzer;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Linq;
using System.Text;

namespace SCI_Lib.Resources.Scripts.Builders;

public class CppBuilder : IScriptBuilder
{
    private readonly StringBuilder sb = new();
    private readonly string _cl;
    private readonly string _method;
    private int _level = 0;
    private ScriptAnalyzer _analyzer;

    public CppBuilder(string cl = null, string method = null)
    {
        _cl = cl;
        _method = method;
    }

    public string Decompile(Script script)
    {
        _analyzer = script.Analyze(_cl, _method);

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
        var name = Expr.ToCppName(s);
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception();

        var super = s.SuperClass;

        if (s.Type == SectionType.Class)
            sb.Append($"class {name}");
        else
            sb.Append($"class");

        if (super != null)
        {
            var sname = Expr.ToCppName(super);
            if (string.IsNullOrWhiteSpace(sname))
                throw new Exception();
            sb.Append($" : {sname}");
        }
        sb.AppendLine()
            .AppendLine("{");
        _level++;

        var pack = s.Package;
        for (int i = 4; i < s.Properties.Length; i++)
        {
            var prop = s.Properties[i];
            var pname = Expr.ToCppName(prop.Name);
            if (prop.Reference is SaidExpression said)
                Space().Append($"said_t {pname} = \"{said.Label}\";");
            else if (prop.Reference is StringConst str)
                Space().Append($"const char * {pname} = \"{str.Value}\";");
            else
            {
                Space().Append($"short {pname} = {prop.Value};");
                if (prop.Value > 9) sb.Append($" // 0x{prop.Value:x4}");
                if (prop.Reference != null)
                    Console.WriteLine($"PROP REF {prop.Reference.GetType().FullName}");
            }
            sb.AppendLine();
        }
        sb.AppendLine();

        for (int i = 0; i < s.FuncNamesInd.Length; i++)
        {
            var addr = s.FuncCode[i].TargetOffset;
            var method = Expr.ToCppName(pack.GetName(s.FuncNamesInd[i]));

            var proc = _analyzer.Procedures.FirstOrDefault(p => p.Class == s && p.Name == method);
            if (proc == null) continue;

            AppendLine($"void {proc.Define}");
            AppendLine("{");
            _level++;

            _level--;
            AppendLine("}");
            sb.AppendLine();
        }

        _level--;
        sb.Append('}');
        if (s.Type == SectionType.Object)
            sb.Append($" {name}");
        sb.AppendLine(";");
        sb.AppendLine();
    }

    private StringBuilder Space()
    {
        for (int i = 0; i < _level; i++)
            sb.Append("    ");
        return sb;
    }

    private void AppendLine(string line) => Space().AppendLine(line);

}
