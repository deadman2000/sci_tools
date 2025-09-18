using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Resources.Scripts1;
using System;
using System.Collections.Generic;

namespace SCI_Lib.Analyzer;

public class ScriptAnalyzer
{
    private readonly string _classFilter;
    private readonly string _methodFilter;
    private readonly HashSet<ushort> _usedCode = new();
    public List<ProcedureTree> Procedures { get; } = new();
    public Dictionary<string, ProcedureTree> Exports { get; } = new();

    public ScriptAnalyzer(Script script, string classFilter, string methodFilter)
    {
        _classFilter = classFilter;
        _methodFilter = methodFilter;
        foreach (var s in script.Get<ClassSection>()) AnalyzeClass(s);
        foreach (var s in script.Get<ExportSection>()) AnalyzeExport(s);
        foreach (var s in script.Get<CodeSection>()) AnalyzeLocal(s);
    }

    public ScriptAnalyzer(Script1 script, string classFilter, string methodFilter)
    {
        _classFilter = classFilter;
        _methodFilter = methodFilter;
        foreach (var obj in script.Objects) AnalyzeObject(obj);
    }

    private void AnalyzeExport(ExportSection s)
    {
        if (_classFilter != null) return;

        for (int i = 0; i < s.Exports.Length; i++)
        {
            var e = s.Exports[i];

            if (e == null) continue;
            if (e.Reference is Code code)
            {
                var exportName = $"scr{s.Script.Resource.Number}_{i}";
                var name = $"localproc_{code.Address:x4}"; // Имя процедуры должно соответствовать имени в вызове. Оператор call
                if (_methodFilter == null || _methodFilter == name)
                {
                    var proc = BuildProc(null, code, name);
                    Exports.Add(exportName, proc);
                }
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
            else
                return;
        }
    }

    private void AnalyzeClass(ClassSection s)
    {
        if (_classFilter != null && s.Name != _classFilter) return;
        s.Prepare();

        for (int i = 0; i < s.FuncNames.Length; i++)
        {
            var addr = s.FuncCode[i].TargetOffset;
            var methodName = s.FuncNames[i];
            if (_methodFilter != null && methodName != _methodFilter) continue;
            Code code = s.Script.GetOperator(addr) as Code;
            BuildProc(s, code, methodName);
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

    private void AnalyzeObject(Object1 obj)
    {
        if (_classFilter != null && obj.Name != _classFilter) return;
        obj.Prepare();

        foreach (var method in obj.Methods)
        {
            if (_methodFilter != null && method.Name != _methodFilter) continue;
            var proc = new ProcedureTree(this, obj, method);
            Procedures.Add(proc);
            proc.BuildMethod();
            _usedCode.Add(method.Address);
        }
    }

    public void Optimize()
    {
        foreach (var proc in Procedures)
            proc.Optimize();
    }
}
