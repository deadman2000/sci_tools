using System;
using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Analyzer;

public class OptimizedTree
{
    private readonly CodeBlock _first;
    private readonly Dictionary<ushort, CodeNode> _nodesByAddr = new();
    private readonly Dictionary<string, VarInfo> _varInfo = new();
    private bool _optimized;

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
        _nodesByAddr.Clear();

        NormalizeExpressions();

        while (true)
        {
            _optimized = false;
            InvertNotCondition();
            CalcVars();
            RemoveUnusedVars();
            RemoveOnceUsedVars();
            ReplaceOnceUsedVars();
            CalcVars();

            OptimizeBranch();
            UnionSimpleBlocks();
            UnionOr();
            UnionAnd();
            if (!_optimized) break;
        }

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

    private void InvertNotCondition()
    {
        // Иневертированные проверки с not
        foreach (var node in Nodes)
        {
            if (node.Condition != null && node.Condition is Math1Expr m && m.Op == "!")
            {
                node.Condition = m.Expression;
                (node.NextB, node.NextA) = (node.NextA, node.NextB);
                _optimized = true;
            }
        }
    }

    private void ReplaceOnceUsedVars()
    {
        // Если переменная один раз присваивается, сравнивается и больше не используется, подставляем сразу в условие
        foreach (var node in Nodes)
        {
            if (node.Condition != null && node.Condition is ParamExpr p && IsVar(p)
                && node.Expressions.Count > 0 && node.Expressions[^1] is SetExpr set && set.A == p)
            {
                if (node.NextA != null && node.NextA.UsedVar(p.Name))
                    continue;
                if (node.NextB != null && node.NextB.UsedVar(p.Name))
                    continue;

                node.Expressions.RemoveAt(node.Expressions.Count - 1);
                node.Condition = set.B;
                _optimized = true;
            }
        }
    }

    private void UnionSimpleBlocks()
    {
        // Объединяем блок с безусловным переходом со следующим блоком, если у него только один родитель
        foreach (var node in Nodes.ToList())
        {
            if (node.Condition == null && node.NextA != null && node.NextA.Parents.Count == 1)
            {
                if (node.NextB != null) throw new Exception();

                var next = node.NextA;
                next.Address = node.Address;
                next.Expressions.InsertRange(0, node.Expressions);
                node.NextA = null;
                foreach (var p in node.Parents.ToList())
                {
                    if (p.NextA == node) p.NextA = next;
                    if (p.NextB == node) p.NextB = next;
                }
                if (node.Parents.Count > 0) throw new Exception();
                RemoveNode(node);
                _optimized = true;
            }
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
                _optimized = true;
            }
        }
    }

    private void RemoveOnceUsedVars()
    {
        // Удаляем переменные которые один раз присвоились и прочитались
        foreach (var (_, info) in _varInfo)
        {
            if (info.Gets == 1 && info.Sets == 1)
            {
                if (info.GetNode == info.SetNode && info.GetInd - 1 == info.SetInd) // Присвоение и чтение должно быть в одной ветке и последновательным
                {
                    RemoveVar(info.Param);
                    Replace(info.Param, info.Value);
                    _optimized = true;
                }
            }
        }
    }

    private void OptimizeBranch()
    {
        // Удаляем лишние переходы и повторные проверки значений переменных
        foreach (var node in Nodes.ToList())
        {
            if (node.Condition != null && node.Condition is ParamExpr p
                && node.Expressions.Count > 0 && node.Expressions[^1] is SetExpr set && set.A == p)
            {
                if (node.NextA != null && node.NextA.Expressions.Count == 0 && node.NextA.Condition is ParamExpr pa && pa.Name == p.Name)
                {
                    node.NextA = node.NextA.NextA;
                    _optimized = true;
                }
                else if (node.NextB != null && node.NextB.Expressions.Count == 0 && node.NextB.Condition is ParamExpr pb && pb.Name == p.Name)
                {
                    node.NextB = node.NextB.NextB;
                    _optimized = true;
                }
            }
        }
    }

    private void UnionOr()
    {
        // Последовательные проверки, true которых ведут к единому блоку, объединяем в ||
        List<CodeNode> orBlocks = new();
        foreach (var node1 in Nodes.ToList())
        {
            if (node1.Condition != null && node1.NextB != null)
            {
                var n = node1.NextB;
                while (true)
                {
                    if (n.Condition != null && n.Expressions.Count == 0 && n.NextA == node1.NextA)
                    {
                        orBlocks.Add(n);
                        if (n.NextB == null) break;
                        n = n.NextB;
                    }
                    else
                        break;
                }
                if (orBlocks.Count > 0)
                {
                    var falseBranch = orBlocks[^1].NextB;
                    Expr ex = node1.Condition;
                    foreach (var node in orBlocks)
                    {
                        ex = new Math2Expr(ex, "||", node.Condition);
                        RemoveNode(node);
                    }
                    node1.Condition = ex;
                    node1.NextB = falseBranch;
                    orBlocks.Clear();
                    _optimized = true;
                }
            }
        }
    }

    private void UnionAnd()
    {
        // Последовательные проверки, false которых ведут к единому блоку, объединяем в &&
        List<CodeNode> orBlocks = new();
        foreach (var node1 in Nodes.ToList())
        {
            if (node1.Condition != null && node1.NextA != null)
            {
                var n = node1.NextA;
                while (true)
                {
                    if (n.Condition != null && n.Expressions.Count == 0 && n.NextB == node1.NextB)
                    {
                        orBlocks.Add(n);
                        if (n.NextA == null) break;
                        n = n.NextA;
                    }
                    else
                        break;
                }
                if (orBlocks.Count > 0)
                {
                    var trueBranch = orBlocks[^1].NextA;
                    Expr ex = node1.Condition;
                    foreach (var node in orBlocks)
                    {
                        ex = new Math2Expr(ex, "&&", node.Condition);
                        RemoveNode(node);
                    }
                    node1.Condition = ex;
                    node1.NextA = trueBranch;
                    orBlocks.Clear();
                    _optimized = true;
                }
            }
        }
    }

    private void RemoveNode(CodeNode node)
    {
        node.Unlink();
        if (node.Parents.Count > 0)
        {
            foreach (var p in node.Parents.ToList())
            {
                if (p.NextA == node) p.NextA = null;
                if (p.NextB == node) p.NextB = null;
            }
        }
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
            node.NextA = CreateNode(bl.BlockA);
        if (bl.BlockB != null)
            node.NextB = CreateNode(bl.BlockB);
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
                CalcVars(node.Expressions[i], node, i);
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
