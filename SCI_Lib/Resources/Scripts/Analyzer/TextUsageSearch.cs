using SCI_Lib.Resources.Scripts.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Resources.Scripts.Analyzer;

public class TextUsageSearch
{
    private readonly SCIPackage _package;
    private readonly ushort? _scr;
    private readonly Dictionary<CodeNode, bool> _passed = new();
    private readonly Dictionary<int, IEnumerable<SaidExpression>> _prints = new();
    private readonly HashSet<string> _printFunc = new();

    public TextUsageSearch(SCIPackage package, ushort? scr = null)
    {
        _package = package;
        _scr = scr;
    }

    public class PrintCall
    {
        public int Script;
        public int Index;
        public SaidExpression[] Saids;
    }

    public IEnumerable<PrintCall> FindUsage()
    {
        if (_scr != null)
        {
            var res = _package.GetResource<ResScript>(_scr.Value);
            FindUsage(res.GetScript() as Script);
        }
        else
        {
            foreach (var res in _package.GetResources<ResScript>())
            {
                //if (res.Number != 0) continue;
                //Console.WriteLine(res.FileName);
                FindUsage(res.GetScript() as Script);
            }
        }

        return _prints.Select(kv => new PrintCall
        {
            Script = (kv.Key >> 16) & 0xffff,
            Index = kv.Key & 0xffff,
            Saids = kv.Value.Distinct().ToArray()
        });
    }

    private void FindUsage(Script script)
    {
        var analyzer = script.Analyze();

        _printFunc.Clear();
        _printFunc.Add("scr255_00");
        foreach (var proc in analyzer.Procedures)
        {
            if (proc.Name.StartsWith("localproc_"))
                if (DetectPrints(proc))
                    AddPrintFunc(proc);
        }

        foreach (var proc in analyzer.Procedures)
        {
            if (proc.Name == "handleEvent")
                FindUsage(proc);
        }

    }

    private void FindUsage(ProcedureTree proc)
    {
        var opt = proc.Optimize();
        if (opt.Node == null) return;
        LookupNode(opt.Node, false, null);
        _passed.Clear();
    }

    private void LookupNode(CodeNode node, bool claimed, IEnumerable<SaidExpression> saids)
    {
        if (_passed.TryGetValue(node, out var cl) && (!cl || claimed)) return;
        _passed[node] = claimed;

        foreach (var ex in node.Expressions)
        {
            if (CheckClaimSet(ex, ref claimed)) continue;
            if (saids != null)
                CheckPrint(ex, saids);
        }

        if (node.Condition != null)
        {
            var condSaids = GetSaids(node.Condition);
            if (condSaids == null) // Обычное условие
            {
                if (node.NextA != null) LookupNode(node.NextA, claimed, saids);
                if (node.NextB != null) LookupNode(node.NextB, claimed, saids);
            }
            else // Said условие
            {
                if (claimed) // Событие уже обработано, идём по false ветке
                {
                    if (node.NextB != null) LookupNode(node.NextB, true, null);
                }
                else
                {
                    claimed = !condSaids.All(e => e.Label.EndsWith(">"));
                    if (node.NextA != null) LookupNode(node.NextA, claimed, Concat(saids, condSaids));
                    if (node.NextB != null) LookupNode(node.NextB, false, saids);
                }
            }
        }
    }

    private void AddPrint(int print, IEnumerable<SaidExpression> saids)
    {
        if (!_prints.TryGetValue(print, out var s))
            _prints.Add(print, saids);
        else
            _prints[print] = s.Concat(saids);
    }

    private void AddPrintFunc(ProcedureTree proc)
    {
        _printFunc.Add(proc.Name);
    }

    private static bool DetectPrints(ProcedureTree proc)
    {
        var opt = proc.Optimize();
        return opt.Nodes.Where(n => n.Expressions.Any(e => IsPrint(e))).Any();
    }

    static bool IsPrint(Expr ex) => ex is CallExpr call && call.Method == "scr255_00";

    private void CheckPrint(Expr ex, IEnumerable<SaidExpression> saids)
    {
        if (ex is CallExpr call && _printFunc.Contains(call.Method)
            && call.Args.Count > 1
            && call.Args[0] is ConstExpr c0
            && call.Args[1] is ConstExpr c1)
        {
            AddPrint((c0.Value << 16) | c1.Value, saids);
        }
    }

    private static bool CheckClaimSet(Expr ex, ref bool claimed)
    {
        if (ex is CallExpr call && call.Method == "claimed" && call.Args.Count > 0 && call.Args[0] is ConstExpr c)
        {
            claimed = c.Value > 0;
            return true;
        }
        return false;
    }

    #region Said search

    private readonly List<SaidExpression> _saids = new();

    private List<SaidExpression> GetSaids(Expr ex)
    {
        _saids.Clear();
        LookupExpr(ex);
        if (_saids.Count > 0) return _saids.ToList();
        return null;
    }

    private void LookupExpr(Expr ex)
    {
        if (ex is CallExpr call && call.Method == "Said" && call.Args[0] is RefExpr r && r.Ref is SaidExpression said)
            _saids.Add(said);
        else if (ex is Math2Expr m2 && m2.Op == "||")
        {
            LookupExpr(m2.A);
            LookupExpr(m2.B);
        }
    }

    #endregion

    private static IEnumerable<SaidExpression> Concat(IEnumerable<SaidExpression> a, IEnumerable<SaidExpression> b)
    {
        if (a == null) return b;
        return a.Concat(b);
    }

}
