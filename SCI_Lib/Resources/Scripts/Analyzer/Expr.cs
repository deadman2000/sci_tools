using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Resources.Scripts.Analyzer;

public abstract class Expr
{
    public int LinksCount { get; set; }
    public virtual bool Used { get; private set; }
    public string VarName { get; private set; }
    public virtual ushort GetValue() => throw new NotImplementedException();
    public abstract string Label { get; }
    public string VarLabel => VarName ?? Label;
    public override string ToString()
    {
        if (VarName != null)
            return $"{GetType().Name}({VarName}:={Label})";
        return $"{GetType().Name}({Label})";
    }

    public void MakeVar(string name) => VarName = name;
    public virtual void Use()
    {
        LinksCount++;
        Used = true;
    }

    public virtual bool IsSimple => false; // true, если выражению не нужна отдельная строка
}

public class ParamExpr : Expr
{
    public string Name { get; }
    public ParamExpr(string name) => Name = name;
    public override string Label => Name;
    public override bool IsSimple => true;
}

public class UShortConst : Expr
{
    public ushort Value { get; }
    public UShortConst(ushort val) => Value = val;
    public UShortConst(int val) : this((ushort)val) { }
    public override ushort GetValue() => Value;
    public override string Label => Value.ToString();
    public override bool IsSimple => true;
}

public class CodeExpr : Expr
{
    public string Code { get; }
    public CodeExpr(string code) => Code = code;
    public override string Label => Code;
}

public class Math1Expr : Expr
{
    public Expr Expression { get; }
    public string Op { get; }

    public Math1Expr(Expr exp, string op)
    {
        exp.Use();
        Expression = exp;
        Op = op;
    }

    public override string Label => $"{Op}({Expression.VarLabel})";
    public override bool IsSimple => true;
}

public class Math2Expr : Expr
{
    public Expr A { get; }
    public Expr B { get; }
    public string Op { get; }
    public Math2Expr(Expr a, string op, Expr b)
    {
        a.Use();
        b.Use();
        A = a;
        B = b;
        Op = op;
    }
    public override string Label => $"{A.VarLabel} {Op} {B.VarLabel}";
    public override bool IsSimple => true;
}

public class SetExpr : Expr
{
    public Expr A { get; }
    public Expr B { get; }
    public SetExpr(ParamExpr a, Expr b)
        : this((Expr)a, b)
    {
    }
    public SetExpr(Expr a, Expr b)
    {
        if (a is ClassExpr) throw new InvalidOperationException();
        b.Use();
        A = a;
        B = b;
    }
    public override string Label => $"{A.VarLabel} = {B.Label}";
}

public class RefExpr : Expr
{
    public RefToElement Ref { get; }
    public RefExpr(RefToElement r) => Ref = r;
    public override string Label => Ref.Reference is SaidExpression s ? $"\"{s}\"" : $"{Ref}";
}

public class CallExpr : Expr
{
    public Expr Target { get; }
    public string Method { get; }
    public List<Expr> Args { get; }
    public CallExpr(string method, List<Expr> args = null)
    {
        Method = method;
        Args = args;
        if (args != null) foreach (var ex in args) ex.Use();
    }
    public CallExpr(Expr target, string method, List<Expr> args = null)
        : this(method, args)
    {
        Target = target;
        target.Use();
    }
    public string GetCall()
    {
        if (Target == null) return Method;
        if (Target is ClassExpr ce)
            return $"{ce.Label}::{Method}";
        else
            return $"{Target.Label}.{Method}";
    }
    public string ArgsStr => Args != null && Args.Count > 0 ? string.Join(", ", Args.Select(e => e.VarLabel)) : string.Empty;
    public override string Label => $"{GetCall()}({ArgsStr})";
}

public class ClassExpr : Expr
{
    public ClassSection Class { get; }
    public string Name { get; }
    public ClassExpr(ClassSection cl)
    {
        Class = cl;
        Name = cl.Name.Replace(' ', '_');
    }
    public ClassExpr(string name) => Name = name;
    public override string Label => Name;
    public override bool IsSimple => true;
}

public class LinkExpr : Expr
{
    private static int NextLinkVar = 0; // TODO REMOVE!!!

    public List<(CodeBlock Block, Expr Expression)> Links { get; } = new();
    public ParamExpr Variable { get; private set; }
    public void SetVar(ParamExpr v) => Variable = v;
    public override string Label
    {
        get
        {
            if (Links.Count == 0)
                return "empty";
            if (Links.Count == 1)
                return Links[0].Expression.Label;

            if (Variable == null)
            {
                Console.WriteLine("LINK VAR SET");
                Variable = new ParamExpr($"vl{NextLinkVar++}");
            }
            return Variable.Name;
        }
    }
    public override bool IsSimple => true;
    public void LinkTo(CodeBlock block, Expr ex) => Links.Add((block, ex));
    public override bool Used
    {
        get
        {
            if (Links.Count == 1)
                return Links[0].Expression.Used;
            return base.Used;
        }
    }
}

public class ArrayExpr : Expr
{
    public ParamExpr Array { get; }
    public Expr Index { get; }
    public ArrayExpr(ParamExpr array, Expr index)
    {
        Array = array;
        Index = index;
        index.Use();
    }
    public override string Label => $"{Array.VarLabel}[{Index.VarLabel}]";
}