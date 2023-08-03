using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System.Collections.Generic;

namespace SCI_Lib.Resources.Scripts.Analyzer;

public class ScriptAnalyzer
{
    private int _procId = 0;
    private readonly string _classFilter;
    private readonly string _methodFilter;
    private readonly HashSet<ushort> _usedCode = new();
    public List<ProcedureTree> Procedures { get; } = new();

    public ScriptAnalyzer(Script script, string classFilter, string methodFilter)
    {
        _classFilter = classFilter;
        _methodFilter = methodFilter;
        foreach (var s in script.Get<ClassSection>()) AnalyzeClass(s);
        foreach (var s in script.Get<ExportSection>()) AnalyzeExport(s);
        foreach (var s in script.Get<CodeSection>()) AnalyzeLocal(s);
    }

    private void AnalyzeExport(ExportSection s)
    {
        if (_classFilter != null) return;

        foreach (var e in s.Exports)
        {
            if (e == null) continue;
            if (e.Reference is Code code)
            {
                var name = $"proc_{s.Script.Resource.Number}_{_procId++}";
                if (_methodFilter == null || _methodFilter == name)
                    BuildProc(null, code, name);
            }
        }
    }

    private void AnalyzeLocal(CodeSection s)
    {
        if (_classFilter != null) return;

        var code = s.Operators[0];
        while (code != null)
        {
            if (_usedCode.Contains(code.Address)) return;
            var name = $"localproc_{code.Address:x4}";

            if (_methodFilter == null || _methodFilter == name)
            {
                var proc = BuildProc(null, code, name);
                if (proc == null) return;
                code = proc.End.Next;
            }
        }
    }

    private void AnalyzeClass(ClassSection s)
    {
        if (_classFilter != null && s.Name != _classFilter) return;
        s.Prepare();

        var pack = s.Package;
        for (int i = 0; i < s.FuncNames.Length; i++)
        {
            var addr = s.FuncCode[i].TargetOffset;
            var method = s.FuncNames[i];
            if (_methodFilter != null && method != _methodFilter) continue;
            Code code = s.Script.GetElement(addr) as Code;
            BuildProc(s, code, method);
        }
    }

    private ProcedureTree BuildProc(ClassSection s, Code code, string method)
    {
        if (code.Type == 0) return null;
        var proc = new ProcedureTree(this, s, code, method);
        Procedures.Add(proc);
        proc.BuildMethod();
        _usedCode.Add(code.Address);
        return proc;
    }
}
