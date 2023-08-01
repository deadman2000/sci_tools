using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCI_Lib.Resources.Scripts.Analyzer;

public class ScriptAnalyzer
{
    public List<ProcedureTree> Procedures { get; } = new();

    public ScriptAnalyzer(Script script)
    {
        foreach (var s in script.Get<ClassSection>(SectionType.Class)) AnalyzeClass(s);
        foreach (var s in script.Get<ObjectSection>()) AnalyzeClass(s);
    }

    private void AnalyzeClass(ClassSection s)
    {
        if (s.Name != "Gaza") { Console.WriteLine("COMMENT ME!"); return; }
        s.Prepare();

        var pack = s.Package;
        for (int i = 0; i < s.FuncNamesInd.Length; i++)
        {
            var addr = s.FuncCode[i].TargetOffset;
            var method = pack.GetName(s.FuncNamesInd[i]);

            if (method != "doit") { Console.WriteLine("COMMENT ME!"); continue; }

            Code code = s.Script.GetElement(addr) as Code;
            var proc = new ProcedureTree(this, s, code, method);
            Procedures.Add(proc);
            proc.BuildMethod();
        }
    }

    public enum CodeType
    {
        ASM, CPP, Meta
    }
    public string GetGraph(CodeType type)
    {
        StringBuilder graphSB = new();

        graphSB.AppendLine("digraph G {")
            .AppendLine("\tgraph [splines=ortho, nodesep=0.8]")
            .AppendLine("\tnode[shape=box fontname=Courier fontsize=20 margin=0.3]");

        foreach (var gr in Procedures.GroupBy(p => p.Class))
        {
            graphSB.AppendLine($"\tsubgraph cluster_{gr.Key.Id:x2} {{");
            graphSB.AppendLine($"\t\tlabel = \"{gr.Key.Name}\"");
            foreach (var proc in gr)
            {
                graphSB.AppendLine($"\t\tsubgraph method_{proc.Name} {{");
                foreach (var bl in proc.Blocks)
                {
                    if (!bl.IsBegin && bl.Parents.Count == 0) continue;

                    bool ret = false;
                    if (bl.BlockA != null)
                        graphSB.AppendLine($"\t\t\t{bl.Label} -> {bl.BlockA.Label} [color=blue]");
                    else if (bl.ReturnA && bl.BlockB != null)
                    {
                        graphSB.AppendLine($"\t\t\t{bl.Label} -> return_{bl.AddrBegin:x4} [color=blue]");
                        ret = true;
                    }

                    if (bl.BlockB != null)
                        graphSB.AppendLine($"\t\t\t{bl.Label} -> {bl.BlockB.Label} [color=red]");
                    else if (bl.ReturnB)
                    {
                        graphSB.AppendLine($"\t\t\t{bl.Label} -> return_{bl.AddrBegin:x4} [color=red]");
                        ret = true;
                    }

                    if (ret)
                        graphSB.AppendLine($"\t\t\treturn_{bl.AddrBegin:x4} [shape=circle label=\"\"]");

                    switch (type)
                    {
                        case CodeType.ASM:
                            graphSB.AppendLine($"\t\t\t{bl.Label} [label=\"{bl.ASM}\"]");
                            break;
                        case CodeType.CPP:
                            graphSB.AppendLine($"\t\t\t{bl.Label} [label=\"{bl.GetCppGraph()}\"]");
                            break;
                        case CodeType.Meta:
                            graphSB.AppendLine($"\t\t\t{bl.Label} [label=\"{bl.GetMetaGraph()}\"]");
                            break;
                    }

                    if (bl.IsBegin)
                        graphSB.AppendLine($"\t\t\tbegin_{bl.AddrBegin:x4} -> {bl.Label}")
                            .AppendLine($"\t\t\tbegin_{bl.AddrBegin:x4} [style=rounded margin=0.1 label=\"{proc.Name}\"]");
                }
                graphSB.AppendLine("\t\t}");
            }
            graphSB.AppendLine("\t}");
        }

        graphSB.AppendLine("}");

        return graphSB.ToString();
    }
}
