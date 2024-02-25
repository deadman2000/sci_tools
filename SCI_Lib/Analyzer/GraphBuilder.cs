using System.Linq;
using System.Text;

namespace SCI_Lib.Analyzer;

public class GraphBuilder
{
    private ScriptAnalyzer _analyzer;
    private StringBuilder sb;
    private StringBuilder sbl;
    private bool _labelHTML;

    public GraphBuilder(ScriptAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    public enum CodeType
    {
        ASM, CPP, Meta, CPP_OPT
    }

    private static string GetLabel(CodeBlock b) => $"Code{b.AddrBegin:x4}";
    private static string GetLabel(CodeNode node) => $"Node{node.Address:x4}";


    public string GetGraph(CodeType type)
    {
        sb = new();
        sbl = new StringBuilder();

        var mr = type == CodeType.Meta ? "0" : "0.3";
        Expr.MetaOut = type == CodeType.Meta;

        sb.AppendLine("digraph G {")
            .AppendLine("\tgraph [splines=ortho, nodesep=0.8]")
            .AppendLine($"\tnode[shape=box fontname=Courier fontsize=20 margin={mr}]");

        foreach (var gr in _analyzer.Procedures.GroupBy(p => p.Class))
        {
            var cl = gr.Key;
            if (cl != null)
            {
                string id;
                if (cl.Name != null)
                    id = Expr.ToCppName(cl.Name);
                else
                    id = $"{cl.Id:x4}";

                sb.AppendLine($"\tsubgraph cluster_{id} {{");
                sb.AppendLine($"\t\tlabel = \"{cl.Name}\"");
            }

            foreach (var proc in gr)
            {
                sb.AppendLine($"\t\tsubgraph cluster_{proc.Name} {{");
                sb.AppendLine($"\t\t\tlabel = \"\"");

                if (type == CodeType.CPP_OPT)
                    WriteOpt(proc);
                else
                    WriteProc(proc, type);
                sb.AppendLine("\t\t}");
            }

            if (cl != null)
                sb.AppendLine("\t}");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private void WriteProc(ProcedureTree proc, CodeType type)
    {
        foreach (var bl in proc.Blocks)
        {
            if (!bl.IsBegin && bl.Parents.Count == 0) continue;
            var label = GetLabel(bl);

            // Links
            if (bl.Condition == null)
            {
                if (bl.BlockA != null)
                    sb.AppendLine($"\t\t\t{label} -> {GetLabel(bl.BlockA)}");
            }
            else
            {
                bool ret = false;
                if (bl.BlockA == null)
                {
                    sb.AppendLine($"\t\t\t{label} -> return_{bl.AddrBegin:x4} [color=blue]");
                    ret = true;
                }
                else
                    sb.AppendLine($"\t\t\t{label} -> {GetLabel(bl.BlockA)} [color=blue]");

                if (bl.BlockB == null)
                {
                    sb.AppendLine($"\t\t\t{label} -> return_{bl.AddrBegin:x4} [color=red]");
                    ret = true;
                }
                else
                    sb.AppendLine($"\t\t\t{label} -> {GetLabel(bl.BlockB)} [color=red]");

                // Return block
                if (ret)
                    sb.AppendLine($"\t\t\treturn_{bl.AddrBegin:x4} [shape=circle label=\"\"]");
            }

            // Block text
            sbl.Clear();
            switch (type)
            {
                case CodeType.ASM:
                    BuildLabelASM(bl);
                    break;
                case CodeType.CPP:
                    BuildLabelCpp(bl);
                    break;
                case CodeType.Meta:
                    BuildLabelMeta(bl);
                    break;
            }
            sb.AppendLine($"\t\t\t{label} [label={sbl}]");

            // Begin block
            if (bl.IsBegin)
            {
                var method = proc.Define;
                if (method.Length > 50) method = method.Substring(0, 50) + "...)";
                sb.AppendLine($"\t\t\tbegin_{bl.AddrBegin:x4} -> {label}")
                    .AppendLine($"\t\t\tbegin_{bl.AddrBegin:x4} [style=rounded margin=0.1 label=\"{method}\"]");
            }
        }
    }

    private void WriteOpt(ProcedureTree proc)
    {
        var opt = proc.Optimize();

        foreach (var node in opt.Nodes)
        {
            var label = GetLabel(node);

            // Links
            if (node.Condition == null)
            {
                if (node.NextA != null)
                    sb.AppendLine($"\t\t\t{label} -> {GetLabel(node.NextA)}");
            }
            else
            {
                bool ret = false;
                if (node.NextA == null)
                {
                    sb.AppendLine($"\t\t\t{label} -> return_{node.Address:x4} [color=blue]");
                    ret = true;
                }
                else
                    sb.AppendLine($"\t\t\t{label} -> {GetLabel(node.NextA)} [color=blue]");

                if (node.NextB == null)
                {
                    sb.AppendLine($"\t\t\t{label} -> return_{node.Address:x4} [color=red]");
                    ret = true;
                }
                else
                    sb.AppendLine($"\t\t\t{label} -> {GetLabel(node.NextB)} [color=red]");

                // Return block
                if (ret)
                    sb.AppendLine($"\t\t\treturn_{node.Address:x4} [shape=circle label=\"\"]");
            }

            // Block text
            sbl.Clear();
            BuildLabel(node);
            sb.AppendLine($"\t\t\t{label} [label={sbl}]");

            // Begin block
            if (node == opt.Node)
            {
                var method = proc.Define;
                if (method.Length > 50) method = method.Substring(0, 50) + "...)";
                sb.AppendLine($"\t\t\tbegin_{node.Address:x4} -> {label}")
                    .AppendLine($"\t\t\tbegin_{node.Address:x4} [style=rounded margin=0.1 label=\"{method}\"]");
            }
        }
    }

    private void LAppendLine(string line, bool red = false)
    {
        if (_labelHTML)
        {
            line = System.Security.SecurityElement.Escape(line);
            if (red) line = $"<font color='red'>{line}</font>";
            sbl.AppendLine($"<tr><td align=\"left\">{line}</td></tr>");
        }
        else
        {
            sbl.Append(line.Replace("\"", "\\\"")).Append("\\l");
        }
    }

    private void BuildLabelASM(CodeBlock bl)
    {
        _labelHTML = false;
        sbl.Append('"');
        LAppendLine($"{bl.AddrBegin:x04}:{bl.AddrEnd:x04}");
        foreach (var c in bl.Code)
            LAppendLine(c.ASM.Trim());
        LAppendLine($"Stack: {bl.Stack.Count()}");
        sbl.Append('"');
    }

    private void BuildLabelCpp(CodeBlock bl)
    {
        _labelHTML = false;
        sbl.Append('"');

        LAppendLine($"// {bl.AddrBegin:x04}:{bl.AddrEnd:x04}");

        foreach (var exp in bl.Expressions)
            LAppendLine(exp.Define);

        if (bl.Condition != null)
            LAppendLine($"if ({bl.Condition})");

        sbl.Append('"');
    }

    private void BuildLabel(CodeNode node)
    {
        _labelHTML = false;
        sbl.Append('"');

        LAppendLine($"// {node.Address:x04}");

        foreach (var exp in node.Expressions)
            LAppendLine(exp.Define);

        if (node.Condition != null)
            LAppendLine($"if ({node.Condition})");

        sbl.Append('"');
    }

    private void BuildLabelMeta(CodeBlock bl)
    {
        _labelHTML = true;
        sbl.AppendLine("<<table border=\"0\" cellborder=\"0\" cellspacing=\"1\">");

        LAppendLine($"{bl.AddrBegin:x04}:{bl.AddrEnd:x04}");

        if (bl.LinkAcc != null)
        {
            LAppendLine($"A0: {bl.LinkAcc}", bl.LinkAcc.Used);
            LAppendLine($"P0: {bl.LinkPrev}", bl.LinkPrev.Used);
            LAppendLine("");
        }

        foreach (var exp in bl.Expressions)
            LAppendLine($"{exp} ->{exp.UseCount}", exp.Used);

        if (bl.Condition != null)
            LAppendLine($"if ({bl.Condition})", bl.Condition.Used);

        LAppendLine("");

        if (bl.Acc != null)
            LAppendLine($"A1: {bl.Acc}", bl.Acc.Used);
        if (bl.Prev != null)
            LAppendLine($"P1: {bl.Prev}", bl.Prev.Used);
        LAppendLine($"Stack: {bl.Stack.Count()}");

        sbl.AppendLine("</table>>");
    }

}
