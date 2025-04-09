using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Analyzer;

public abstract class Expr
{
    public CodeBlock Block { get; set; }
    public List<Expr> Args { get; set; }
    public int UseCount { get; set; }
    public virtual bool Used { get; private set; }
    public ParamExpr Var { get; set; }
    public virtual short GetValue() => throw new NotImplementedException();
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
    public virtual string Braces => ToString();
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

    public static string ToCppName(string name) => name
        .Replace('-', '_')
        .Replace('\'', '_')
        .Replace(' ', '_')
        .Replace('.', '_')
        .Replace('!', '_');

    public static string ToCppName(ClassSection cl)
    {
        if (cl.Name != null) return ToCppName(cl.Name);
        return $"Class{cl.Id:x2}";
    }
}

public class ParamExpr : Expr
{
    public string Type { get; set; } = "short";
    public string Name { get; }
    public ParamExpr(string name) => Name = ToCppName(name);
    public ParamExpr(PropertyElement prop) => Name = ToCppName(prop.Name);
    public ParamExpr(ClassSection cl) => Name = ToCppName(cl);
    public override string Label => Name;
}

public class ConstExpr : Expr
{
    public short Value { get; }
    public ConstExpr(short val) => Value = val;
    public ConstExpr(int val) : this((short)val) { }
    public override string Label => Value.ToString();
    public override short GetValue() => Value;
}

public class Math1Expr : Expr
{
    public Expr Expression => Args[0];
    public string Op { get; }
    public Math1Expr(Expr exp, string op)
    {
        Args = new() { exp };
        Op = op;
    }
    public override string Label => $"{Op}{Expression.Braces}";
    public override string Braces => $"({this})";
    public override void Use()
    {
        if (!Used)
            Expression.Use();
        base.Use();
    }
}

public class Math2Expr : Expr
{
    public Expr A => Args[0];
    public Expr B => Args[1];
    public string Op { get; set; }
    public Math2Expr(Expr a, string op, Expr b)
    {
        Args = new() { a, b };
        Op = op;
    }
    public override string Label => $"{A.Braces} {Op} {B.Braces}";
    public override string Braces => $"({this})";
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
    public Expr B => Args[0];
    public SetExpr(Expr a, Expr b, bool external) // если external, помечаем все операции как использованные
    {
        if (a is ClassExpr) throw new InvalidOperationException();
        A = a;
        Args = new() { b };
        if (external) b.Use();
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
    public override string Braces => $"({this})";
}

public class RefExpr : Expr
{
    public BaseElement Ref { get; }
    public RefExpr(BaseRef r) => Ref = r.Reference;

    public override string Label => Ref switch
    {
        SaidExpression s => $"\"{s.Label}\"",
        StringConst str => $"\"{str.ValueSlashEsc}\"",
        _ => throw new NotImplementedException()
    };
}

public class CallExpr : Expr
{
    public Expr Target { get; }
    public string Method { get; }
    public CallExpr(string method, List<Expr> args = null)
    {
        Method = ToCppName(method);
        Args = args;
        if (args != null)
            foreach (var ex in args) ex.Use();
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
        base.Use();
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
        Name = ToCppName(cl);
    }
    public ClassExpr(string name) => Name = name;
    public override string Label => Name;
}

public class LinkExpr : Expr
{
    static int NextId = 0;
    public int Id { get; } = NextId++;
    public bool IsAcc { get; }
    public string Description { get; }
    public List<(CodeBlock Block, Expr Expression)> Links { get; } = new();
    public ParamExpr Variable { get; private set; } // TODO Заменить на Var
    public bool Checked { get; internal set; }
    public override string Label
    {
        get
        {
            if (!MetaOut)
            {
                if (Variable != null) return Variable.ToString();
            }

            if (Links.Count == 0) return $"_{Id}[0]";
            if (Links.Count == 1) return $"_{Id}({Links[0].Expression})";
            //Variable ??= new ParamExpr($"link{Id}");
            return $"_{Id}[{Links.Count}]";
        }
    }
    public LinkExpr(bool isAcc, string descr, CodeBlock owner)
    {
        IsAcc = isAcc;
        Description = descr;
        Block = owner;
    }
    public void SetVar(ParamExpr v) => Variable = v;
    public void LinkTo(CodeBlock block, Expr ex) => Links.Add((block, ex));
}

public class ArrayExpr : Expr
{
    public ParamExpr Array => (ParamExpr)Args[0];
    public Expr Index => Args[1];
    public ArrayExpr(ParamExpr array, Expr index) => Args = new() { array, index };
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