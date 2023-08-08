using System;
using System.Collections.Generic;

namespace SCI_Lib.Analyzer;

public class CodeNode
{
    private CodeNode _nextA;
    private CodeNode _nextB;

    public List<Expr> Expressions { get; set; }
    public Expr Condition { get; set; }
    public CodeNode NextA
    {
        get => _nextA;
        set
        {
            if (_nextA == value) return;
            _nextA?.Parents.Remove(this);
            _nextA = value;
            _nextA?.Parents.Add(this);
        }
    }

    public CodeNode NextB
    {
        get => _nextB;
        set
        {
            if (_nextB == value) return;
            _nextB?.Parents.Remove(this);
            _nextB = value;
            _nextB?.Parents.Add(this);
        }
    }
    public List<CodeNode> Parents { get; } = new();
    public ushort Address { get; set; }

    internal void RemoveSet(Expr param)
    {
        for (int i = 0; i < Expressions.Count; i++)
        {
            var e = Expressions[i];
            if (e == null) continue;
            if (e is SetExpr s && s.A == param)
            {
                Expressions.RemoveAt(i);
                --i;
            }
        }
    }

    internal void Replace(Expr a, Expr b)
    {
        for (int i = 0; i < Expressions.Count; i++)
        {
            if (Expressions[i] == null) continue;

            if (Expressions[i] == a)
                Expressions[i] = b;
            else
                ReplaceArg(Expressions[i], a, b);
        }
        if (Condition != null)
        {
            if (Condition == a)
                Condition = b;
            else
                ReplaceArg(Condition, a, b);
        }
    }

    internal void Unlink()
    {
        NextA = null;
        NextB = null;
    }

    private void ReplaceArg(Expr e, Expr a, Expr b)
    {
        if (e is LinkExpr l)
        {
            ReplaceLinks(l, a, b);
            return;
        }

        if (e.Args == null) return;
        for (int i = 0; i < e.Args.Count; i++)
        {
            if (e.Args[i] == a)
                e.Args[i] = b;
            else
                ReplaceArg(e.Args[i], a, b);
        }
    }
    private void ReplaceLinks(LinkExpr e, Expr a, Expr b)
    {
        for (int i = 0; i < e.Links.Count; i++)
        {
            if (e.Links[i].Expression == a)
                e.Links[i] = (e.Links[i].Block, b);
            else
                ReplaceArg(e.Links[i].Expression, a, b);
        }
    }

    public override string ToString()
    {
        return $"{Address:x4}";
    }

    /// <summary>
    /// Проверяем что переменная используется в этом блоке или в следующих
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool UsedVar(string name)
    {
        HashSet<CodeNode> chckd = new(); // Защита от зацикливания
        return UsedVar(name, chckd);
    }


    private bool UsedVar(string name, HashSet<CodeNode> chckd)
    {
        chckd.Add(this);

        foreach (var exp in Expressions)
        {
            if (exp is SetExpr s)
            {
                if (s.A is ParamExpr pa)
                    if (pa.Name == name)
                        return false;
            }

            if (UsedVar(exp, name)) return true;
        }
        if (Condition != null)
        {
            if (UsedVar(Condition, name)) return true;
        }

        if (NextA != null && !chckd.Contains(NextA))
            if (NextA.UsedVar(name, chckd)) return true;

        if (NextB != null && !chckd.Contains(NextB))
            if (NextB.UsedVar(name, chckd)) return true;

        return false;
    }

    private bool UsedVar(Expr exp, string name)
    {
        if (exp is ParamExpr pe)
        {
            if (pe.Name == name) return true;
        }

        if (exp.Args != null)
        {
            foreach (var a in exp.Args)
            {
                if (UsedVar(a, name)) return true;
            }
        }
        return false;
    }
}
