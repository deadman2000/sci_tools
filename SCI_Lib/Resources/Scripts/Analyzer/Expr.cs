using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Resources.Scripts.Analyzer;

public abstract class Expr
{
    public CodeBlock Block { get; set; }
    public int UseCount { get; set; }
    public virtual bool Used { get; private set; }
    public ParamExpr Var { get; private set; }
    public virtual ushort GetValue() => throw new NotImplementedException();
    public abstract string Label { get; }

    public static bool MetaOut = true;
    public override string ToString()
    {
        if (MetaOut)
        {
            if (Var != null)
                return $"{GetType().Name}({Var.Name}:={Label})";
            return $"{GetType().Name}({Label})";
        }
        if (Var != null) return Var.Name;
        return Label;
    }
    public string Define => Var != null ? $"{Var.Name} = {Label}" : Label;

    public void MakeVar(ParamExpr var)
    {
        if (Var != null) throw new InvalidOperationException();
        Var = var;
    }

    public virtual void Use()
    {
        UseCount++;
        Used = true;
    }
}

public class ParamExpr : Expr
{
    public string Name { get; }
    public ParamExpr(string name) => Name = name;
    public override string Label => Name;
}

public class ConstExpr : Expr
{
    public ushort Value { get; }
    public ConstExpr(ushort val) => Value = val;
    public ConstExpr(int val) : this((ushort)val) { }
    public override ushort GetValue() => Value;
    public override string Label => Value.ToString();
}

public class Math1Expr : Expr
{
    public Expr Expression { get; }
    public string Op { get; }
    public Math1Expr(Expr exp, string op)
    {
        Expression = exp;
        Op = op;
    }
    public override string Label => $"{Op}({Expression})";
    public override void Use()
    {
        if (!Used)
            Expression.Use();
        base.Use();
    }
}

public class Math2Expr : Expr
{
    public Expr A { get; }
    public Expr B { get; }
    public string Op { get; }
    public Math2Expr(Expr a, string op, Expr b)
    {
        A = a;
        B = b;
        Op = op;
    }
    public override string Label => $"{A} {Op} {B}";
    public override void Use()
    {
        if (!Used)
        {
            A.Use();
            B.Use();
        }
        base.Use();
    }
}

public class SetExpr : Expr
{
    public Expr A { get; }
    public Expr B { get; }
    public SetExpr(Expr a, Expr b, bool external) // если external, помечаем все операции как использованные
    {
        if (a is ClassExpr) throw new InvalidOperationException();
        if (external) b.Use();
        A = a;
        B = b;
    }
    public override string Label
    {
        get
        {
            if (MetaOut)
                return $"{A} = {B}";
            if (B.Var != A)
                return $"{A} = {B}";
            return $"{A} = {B.Label}";
        }
    }
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
    public string ArgsStr => Args != null && Args.Count > 0 ? string.Join(", ", Args.Select(e => e.ToString())) : string.Empty;
    public override string Label => $"{GetCall()}({ArgsStr})";
    public override void Use()
    {
        if (Var != null) return;
        MakeVar(Block.Procedure.CreateVar());
    }
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
}

public class LinkExpr : Expr
{
    static int NextId = 0;
    private int _id = NextId++;
    public bool IsAcc { get; }
    public string Description { get; }
    public List<(CodeBlock Block, Expr Expression)> Links { get; } = new();
    public ParamExpr Variable { get; private set; }
    public bool Checked { get; internal set; }
    public override string Label
    {
        get
        {
            if (Links.Count == 0) return "empty";
            if (Links.Count == 1) return Links[0].Expression.ToString();
            Variable ??= new ParamExpr($"_{_id}[{Links.Count}]");
            return Variable.Name;
        }
    }
    public LinkExpr(bool isAcc, string descr)
    {
        IsAcc = isAcc;
        Description = descr;
    }
    public void SetVar(ParamExpr v) => Variable = v;
    public void LinkTo(CodeBlock block, Expr ex) => Links.Add((block, ex));
}

public class ArrayExpr : Expr
{
    public ParamExpr Array { get; }
    public Expr Index { get; }
    public ArrayExpr(ParamExpr array, Expr index)
    {
        Array = array;
        Index = index;
    }
    public override void Use()
    {
        if (!Used)
        {
            Array.Use();
            Index.Use();
        }
        base.Use();
    }
    public override string Label => $"{Array}[{Index}]";
}