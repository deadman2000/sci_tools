using System;
using System.Collections.Generic;

namespace SCI_Lib.Resources.Scripts.Analyzer;

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
}
