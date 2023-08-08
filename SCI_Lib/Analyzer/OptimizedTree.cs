using System;
using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Analyzer;

public class OptimizedTree
{
    private readonly CodeBlock _first;
    private readonly Dictionary<ushort, CodeNode> _nodesByAddr = new();
    private readonly Dictionary<string, VarInfo> _varInfo = new();

    public OptimizedTree(CodeBlock first)
    {
        _first = first;
    }

    public CodeNode Node { get; private set; }
    public List<CodeNode> Nodes { get; private set; }

    public void Optimize()
    {
        Expr.MetaOut = true;
        if (Node != null) return;
        Nodes = new();
        Node = CreateNode(_first);

        NormalizeExpressions();

        CalcVars();
        RemoveUnusedVars();
        RemoveOnceUsedVars();
        CalcVars();
        OrPattern();

        //CalcVars();
        //foreach (var (name, info) in _varInfo) Console.WriteLine($"{name} \t{info.Sets} \t{info.Gets}");
    }

    private void NormalizeExpressions()
    {
        foreach (var node in Nodes)
        {
            for (int i = 0; i < node.Expressions.Count; i++)
            {
                var ex = node.Expressions[i];
                if (ex is SetExpr set)
                {
                    node.Expressions[i] = null;
                    Replace(set.B, set.A);
                    if (set.B.Var != null)
                        set.B.Var = null;
                    node.Expressions[i] = set;
                    ex = set;
                }
                else if (ex.Var != null) // Заменяем выражения на переменные
                {
                    var s = new SetExpr(ex.Var, ex, false);
                    node.Expressions[i] = null;
                    Replace(ex, ex.Var);
                    node.Expressions[i] = s;
                    ex.Var = null;
                    ex = s;
                }
                else if (ex is LinkExpr l)
                {
                    if (l.Variable != null)
                        Replace(l, l.Variable);
                    else
                        throw new Exception();
                }

                Normalize(ex);
            }

            if (node.Condition != null)
            {
                if (node.Condition is LinkExpr lc)
                {
                    if (lc.Variable != null)
                        Replace(lc, lc.Variable);
                    else if (lc.Links.Count == 1)
                        node.Condition = lc.Links[0].Expression;
                    else
                        throw new Exception();
                }
                Normalize(node.Condition);
            }
        }

        // Проверяем, что вычистили все линки
        foreach (var node in Nodes)
        {
            foreach (var ex in node.Expressions)
                if (HasLink(ex))
                {
                    Console.WriteLine($"Link still used!! {ex}");
                    //throw new Exception();
                }

            if (node.Condition != null && HasLink(node.Condition))
                throw new Exception();
        }
    }

    private void Normalize(Expr ex)
    {
        if (ex.Args == null) return;
        for (int i = 0; i < ex.Args.Count; i++)
        {
            if (ex.Args[i] is LinkExpr l)
            {
                if (l.Variable != null)
                    ex.Args[i] = l.Variable;
                else if (l.Links.Count == 1)
                    ex.Args[i] = l.Links[0].Expression;
                else
                    Console.WriteLine($"USED LINK WITHOUT VAR {ex}");
            }
            Normalize(ex.Args[i]);
        }
    }

    private void RemoveUnusedVars()
    {
        foreach (var (_, info) in _varInfo)
        {
            if (info.Gets == 0)
            {
                // Удаляем неиспользуемую переменную
                RemoveVar(info.Param);
                Replace(info.Param, info.Value);
            }
        }
    }

    private void RemoveOnceUsedVars()
    {
        foreach (var (_, info) in _varInfo)
        {
            if (info.Gets == 1 && info.Sets == 1)
            {
                if (info.GetNode == info.SetNode && info.GetInd - 1 == info.SetInd) // Присвоение и чтение должно быть в одной ветке и последновательным
                {
                    RemoveVar(info.Param);
                    Replace(info.Param, info.Value);
                }
            }
        }
    }

    private void OrPattern()
    {
        /*
         * a = e1            node1
         * if (!a) { _______/
         *  a = e2   --------- node2
         * }
         * if (a) e3 else e4 - node3
         * 
         * меняем на:
         * if (e1 || e2) e3 else e4
         */
        // a  sets=1 gets=2

        foreach (var node1 in Nodes.ToList())
        {
            if (!(node1.Expressions.Count > 0 && node1.Condition != null && node1.Condition is ParamExpr p)) continue;
            if (!IsVar(p)) continue;

            var info = _varInfo[p.Name];
            if (!(info.Gets == 2 && info.Sets == 2)) continue;

            var node2 = node1.NextB;
            var node3 = node1.NextA;
            if (!(node2.Condition == null && node2.Expressions.Count == 1
                && node3.Condition != null && node3.Expressions.Count == 0)) continue;

            if (node1.Expressions[^1] is not SetExpr set1) continue;
            if (node2.Expressions[0] is not SetExpr set2) continue;
            if (node3.Condition != p) continue;

            var e1 = set1.B;
            var e2 = set2.B;

            RemoveVar(p);
            node1.Condition = new Math2Expr(e1, "||", e2);
            node1.NextA = node3.NextA;
            node1.NextB = node3.NextB;

            node2.Unlink();
            node3.Unlink();
            RemoveNode(node2);
            RemoveNode(node3);
        }

        /*foreach (var (_, infoA) in _varInfo)
        {
            if (!(infoA.Sets == 2 && infoA.Gets == 2)) continue;

            var node1 = infoA.SetNode;
            var node2 = node1.NextB;
            var node3 = node1.NextA;

            if (!(node2.Condition == null && node2.Expressions.Count == 1)) continue;
            if (!(node3.Condition != null && node3.Expressions.Count == 0)) continue;
            if (node2.Expressions[0] is not SetExpr set) continue;

            var e1 = infoA.Value;
            var e2 = set.B;

            RemoveVar(infoA.Param);
            RemoveVar(infoB.Param);
            node1.Condition = new Math2Expr(e1, "||", e2);
            node1.NextA = node3.NextA;
            node1.NextB = node3.NextB;
            RemoveNode(node2);
            RemoveNode(node3);
        }*/
    }

    private void RemoveNode(CodeNode node)
    {
        _nodesByAddr.Remove(node.Address);
        Nodes.Remove(node);
    }

    private CodeNode CreateNode(CodeBlock bl)
    {
        if (_nodesByAddr.TryGetValue(bl.AddrBegin, out var node)) return node;

        if (bl.Expressions.Count == 0 && bl.Condition == null)
        {
            if (bl.BlockA == null) return null;
            if (bl.BlockA != bl)
                return CreateNode(bl.BlockA);
        }

        node = new CodeNode();
        node.Expressions = bl.Expressions;
        node.Condition = bl.Condition;
        node.Address = bl.AddrBegin;
        _nodesByAddr.Add(bl.AddrBegin, node);
        Nodes.Add(node);

        if (bl.BlockA != null)
        {
            node.NextA = CreateNode(bl.BlockA);
        }
        if (bl.BlockB != null)
        {
            node.NextB = CreateNode(bl.BlockB);
        }
        return node;
    }

    private bool HasLink(Expr ex)
    {
        if (ex is LinkExpr) return true;
        if (ex.Args != null)
        {
            foreach (var a in ex.Args)
                if (HasLink(a)) return true;
        }
        return false;
    }

    private void Replace(Expr a, Expr b)
    {
        foreach (var node in Nodes)
            node.Replace(a, b);
    }

    private void RemoveVar(ParamExpr param)
    {
        //Console.WriteLine($"Remove {param.Name}");
        _varInfo.Remove(param.Name);
        foreach (var node in Nodes)
            node.RemoveSet(param);
    }

    private void CalcVars()
    {
        _varInfo.Clear();
        foreach (var node in Nodes)
        {
            for (int i = 0; i < node.Expressions.Count; i++)
            {
                CalcVars(node.Expressions[i], node, i);
            }
            if (node.Condition != null)
                CalcVars(node.Condition, node, node.Expressions.Count);
        }
    }

    private void CalcVars(Expr exp, CodeNode node, int ind)
    {
        if (exp is SetExpr s)
        {
            if (s.A is ParamExpr pa)
            {
                if (IsVar(pa))
                {
                    Var(pa).Value = s.B;
                    IncVarSet(pa, node, ind);
                }
            }
        }
        else if (exp is ParamExpr pe)
        {
            if (IsVar(pe))
                IncVarGet(pe, node, ind);
        }

        if (exp.Args != null)
        {
            foreach (var a in exp.Args)
            {
                CalcVars(a, node, ind);
            }
        }
    }

    private static bool IsVar(ParamExpr p)
    {
        return p.Name.StartsWith("var") || p.Name.StartsWith("tmp");
    }

    class VarInfo
    {
        public int Gets = 0;
        public int Sets = 0;
        public ParamExpr Param;
        public Expr Value;
        public CodeNode GetNode;
        public int GetInd;
        public CodeNode SetNode;
        public int SetInd;
    }

    private VarInfo Var(ParamExpr p)
    {
        if (_varInfo.TryGetValue(p.Name, out var val)) return val;
        val = new VarInfo();
        val.Param = p;
        _varInfo.Add(p.Name, val);
        return val;
    }

    private void IncVarGet(ParamExpr p, CodeNode node, int ind)
    {
        var info = Var(p);
        info.Gets++;
        if (info.Gets == 1)
        {
            info.GetNode = node;
            info.GetInd = ind;
        }
    }

    private void IncVarSet(ParamExpr p, CodeNode node, int ind)
    {
        var info = Var(p);
        info.Sets++;
        if (info.Sets == 1)
        {
            info.SetNode = node;
            info.SetInd = ind;
        }
    }

}
