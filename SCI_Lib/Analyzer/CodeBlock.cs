using SCI_Lib.Resources.Scripts.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Analyzer;

public class CodeBlock
{
    private readonly SCIPackage _package;

    public ProcedureTree Procedure { get; }
    public List<Code> Code { get; }
    public bool IsBegin { get; }

    public List<Expr> Expressions { get; } = new();
    private readonly Stack<Expr> _stack = new();
    private Stack<Expr> _prevStack;

    private bool _isBuild;
    private bool _isDecompiled;

    // Ссылки для сбора значений аккумуляторов, которые могут прийти из других блоков
    public LinkExpr LinkAcc { get; }
    public LinkExpr LinkPrev { get; }

    public Expr Prev { get; private set; }
    public Expr Acc { get; private set; }
    public Expr Condition { get; private set; }
    private ushort _rest;
    private bool _mainUsed;

    public List<CodeBlock> Parents { get; } = new();

    public Code NextA { get; set; }
    public CodeBlock BlockA { get; set; }

    public Code NextB { get; set; }
    public CodeBlock BlockB { get; set; }


    public ushort AddrBegin => Code[0].Address;
    public ushort AddrEnd => Code[^1].Address;
    public bool ReturnA => NextA != null && NextA.IsReturn;
    public bool ReturnB => NextB != null && NextB.IsReturn;

    public IEnumerable<Expr> Stack => _stack;


    public CodeBlock(ProcedureTree proc, List<Code> list, bool isBegin)
    {
        if (!list.Any()) throw new ArgumentException("Empty collection", nameof(list));

        Procedure = proc;
        Code = list;
        IsBegin = isBegin;

        _package = list[0].Script.Package;
        //if (!isBegin)
        {
            LinkAcc = new LinkExpr(true, $"A {AddrBegin:x4}", this);
            LinkPrev = new LinkExpr(false, $"P {AddrBegin:x4}", this);
            Acc = LinkAcc;
            Prev = LinkPrev;
            proc.RegisterLink(LinkAcc);
            proc.RegisterLink(LinkPrev);
        }
    }

    public override string ToString() => $"{AddrBegin:x4} -> ({NextA?.Address:x4}, {NextB?.Address:x4})";


    public void BuildTree()
    {
        if (_isBuild) return;

        CalcNext();
        _isBuild = true;

        if (NextA != null && !NextA.IsReturn)
        {
            BlockA = Procedure.GetBlock(NextA);
            BlockA.Parents.Add(this);
            BlockA.BuildTree();
        }
        if (NextB != null && !NextB.IsReturn)
        {
            BlockB = Procedure.GetBlock(NextB);
            BlockB.Parents.Add(this);
            BlockB.BuildTree();
        }
    }

    private void CalcNext()
    {
        var last = Code[^1];

        if (last.IsReturn)
        {
            NextA = last;
            return;
        }

        if (last.Name == "jmp")
        {
            var r = last.Arguments[0] as CodeRef;
            NextA = (Code)r.Reference;
            return;
        }

        switch (last.Type)
        {
            case 0x2e: // bt
            case 0x2f:
                {
                    var r = last.Arguments[0] as CodeRef;
                    NextA = (Code)r.Reference;
                    NextB = last.Next;
                }
                break;
            case 0x30: // bnt
            case 0x31:
                {
                    var r = last.Arguments[0] as CodeRef;
                    NextB = (Code)r.Reference;
                    NextA = last.Next;
                }
                break;
            default:
                NextA = last.Next;
                break;
        }
    }

    int StackSize
    {
        get
        {
            if (_prevStack != null) return _prevStack.Count + _stack.Count;
            return _stack.Count;
        }
    }

    public void Decompile()
    {
        if (_isDecompiled) return;
        _isDecompiled = true;

        foreach (var c in Code)
            ExecuteCode(c);

        if (_mainUsed) Procedure.Using(0);

        if (Condition != null && Condition.Var == null)
            Expressions.Remove(Condition);

        if (BlockA != null)
        {
            if (!BlockA._isDecompiled)
            {
                BlockA._prevStack = GetStackTail();
                BlockA.Decompile();
            }

            BlockA.LinkTo(this);
        }

        if (BlockB != null)
        {
            if (!BlockB._isDecompiled)
            {
                BlockB._prevStack = GetStackTail();
                BlockB.Decompile();
            }

            BlockB.LinkTo(this);
        }
    }

    private Stack<Expr> GetStackTail()
    {
        if (_prevStack == null)
            return new(_stack.Reverse());
        return new(_prevStack.Reverse().Concat(_stack.Reverse()));
    }

    private void LinkTo(CodeBlock parent)
    {
        if (parent.Acc != null) LinkAcc.LinkTo(parent, parent.Acc);
        if (parent.Prev != null) LinkPrev.LinkTo(parent, parent.Prev);
    }

    private void ExecuteCode(Code code)
    {
        if (code.Type >= 0x80)
        {
            ushort val = GetVal(code.Arguments[0]);
            var type = code.Type >> 1 & 0x3;
            bool isStack = (code.Type & 1 << 3) != 0;
            bool isAccIndex = (code.Type & 1 << 4) != 0;

            Expr variable = OpGetExpression(type, val);
            if (isAccIndex)
                variable = new ArrayExpr((ParamExpr)variable, GetAcc());

            var exec = code.Type >> 5 & 0x3;

            switch (exec)
            {
                case 0: // Load
                    break;
                case 2: // Increment
                    {
                        var exp = new Math2Expr(variable, "+", new ConstExpr(1));
                        Set(variable, exp, true);
                        variable = exp;
                    }
                    break;
                case 3: // Decrement
                    {
                        var exp = new Math2Expr(variable, "-", new ConstExpr(1));
                        Set(variable, exp, true);
                        variable = exp;
                    }
                    break;
                case 1: // Store
                    {
                        Expr op;
                        if (isStack)
                            op = Pop();
                        else
                        {
                            if (isAccIndex)
                                op = Pop();
                            else
                                op = GetAcc();
                        }
                        Set(variable, op, true);
                    }
                    return;
            }

            if (isStack)
                Push(variable);
            else
                SetAcc(variable);
            return;
        }

        switch (code.Type)
        {
            case 0x00: // not
            case 0x01:
                SetAcc(new Math1Expr(GetAcc(), "~"));
                break;
            case 0x02: // add
            case 0x03:
                SetAcc(new Math2Expr(Pop(), "+", GetAcc()));
                break;
            case 0x04: // sub
            case 0x05:
                SetAcc(new Math2Expr(Pop(), "-", GetAcc()));
                break;
            case 0x06: // mul
            case 0x07:
                SetAcc(new Math2Expr(Pop(), "*", GetAcc()));
                break;
            case 0x08: // div
            case 0x09:
                SetAcc(new Math2Expr(Pop(), "/", GetAcc()));
                break;
            case 0x0a: // mod
            case 0x0b:
                SetAcc(new Math2Expr(Pop(), "%", GetAcc()));
                break;
            case 0x0c: // shr
            case 0x0d:
                SetAcc(new Math2Expr(Pop(), ">>", GetAcc()));
                break;
            case 0x0e: // shl
            case 0x0f:
                SetAcc(new Math2Expr(Pop(), "<<", GetAcc()));
                break;
            case 0x10: // xor
            case 0x11:
                SetAcc(new Math2Expr(Pop(), "^", GetAcc()));
                break;
            case 0x12: // and
            case 0x13:
                SetAcc(new Math2Expr(Pop(), "&", GetAcc()));
                break;
            case 0x14: // or
            case 0x15:
                SetAcc(new Math2Expr(Pop(), "|", GetAcc()));
                break;
            case 0x16: // neg
            case 0x17:
                SetAcc(new Math1Expr(GetAcc(), "-"));
                break;
            case 0x18: // not
            case 0x19:
                SetAcc(new Math1Expr(GetAcc(), "!"));
                break;
            case 0x1a: // eq?
            case 0x1b:
                Prev = GetAcc();
                SetAcc(new Math2Expr(Pop(), "==", GetAcc()));
                break;
            case 0x1c: // ne?
            case 0x1d:
                Prev = GetAcc();
                SetAcc(new Math2Expr(Pop(), "!=", GetAcc()));
                break;
            case 0x1e: // gt?
            case 0x1f:
                Prev = GetAcc();
                SetAcc(new Math2Expr(Pop(), ">", GetAcc()));
                break;
            case 0x20: // ge?
            case 0x21:
                Prev = GetAcc();
                SetAcc(new Math2Expr(Pop(), ">=", GetAcc()));
                break;
            case 0x22: // lt?
            case 0x23:
                Prev = GetAcc();
                SetAcc(new Math2Expr(Pop(), "<", GetAcc()));
                break;
            case 0x24: // le?
            case 0x25:
                Prev = GetAcc();
                SetAcc(new Math2Expr(Pop(), "<=", GetAcc()));
                break;
            case 0x26: // ugt?
            case 0x27:
                SetAcc(new Math2Expr(Pop(), ">", GetAcc()));
                break;
            case 0x28: // uge?
            case 0x29:
                SetAcc(new Math2Expr(Pop(), ">=", GetAcc()));
                break;
            case 0x2a: // ult?
            case 0x2b:
                SetAcc(new Math2Expr(Pop(), "<", GetAcc()));
                break;
            case 0x2c: // ule?
            case 0x2d:
                SetAcc(new Math2Expr(Pop(), ">=", GetAcc()));
                break;
            case 0x2e: // bt
            case 0x2f:
            case 0x30: // bnt
            case 0x31:
                Condition = GetAcc();
                Condition.Use();
                if (code != Code[^1]) throw new Exception();
                break;
            case 0x32: //jmp
            case 0x33:
                if (code != Code[^1]) throw new Exception();
                break;
            case 0x34: // ldi W
            case 0x35: // ldi B
                SetAcc(GetVal(code.Arguments[0]));
                break;
            case 0x36: // push
            case 0x37:
                Push(GetAcc());
                break;
            case 0x38: // pushi W
            case 0x39: // pushi B
                Push(GetVal(code.Arguments[0]));
                break;
            case 0x3a: // toss
            case 0x3b:
                Pop();
                break;
            case 0x3c: // dup
            case 0x3d:
                Push(TOS());
                break;
            case 0x3e: // link
            case 0x3f:
                {
                    ushort cnt = GetVal(code.Arguments[0]);
                    for (ushort i = 0; i < cnt; i++)
                        Push(Procedure.GetParam(i));
                    Push(cnt);
                }
                break;
            case 0x40: // call
            case 0x41:
                {
                    var cr = (CodeRef)code.Arguments[0];
                    var proc = $"localproc_{cr.TargetOffset:x4}";
                    var cnt = (byte)code.Arguments[1] / 2;
                    var args = PopArgs(cnt);
                    SetAcc(new CallExpr(proc, args));
                }
                break;
            case 0x42: // callk
            case 0x43:
                {
                    ushort fun = GetVal(code.Arguments[0]);
                    string method = _package.GetFuncName(fun);
                    //string method = Kernel.GetFunc(fun);
                    var cnt = (byte)code.Arguments[1] / 2;
                    var args = PopArgs(cnt);
                    SetAcc(new CallExpr(method, args));
                }
                break;
            case 0x44: // callb
            case 0x45:
                {
                    _mainUsed = true;
                    ushort ind = GetVal(code.Arguments[0]);
                    var cnt = (byte)code.Arguments[1] / 2;
                    var args = PopArgs(cnt);
                    SetAcc(new CallExpr($"proc_{ind}", args)); // индекс - номер процедуры из экспорта скрипта 0
                }
                break;
            case 0x46: // calle
            case 0x47:
                {
                    ushort scr = GetVal(code.Arguments[0]);
                    Procedure.Using(scr);
                    var ind = GetVal(code.Arguments[1]);
                    var cnt = (byte)code.Arguments[2] / 2;
                    var args = PopArgs(cnt);
                    SetAcc(new CallExpr($"scr{scr}_{ind:x2}", args));
                }
                break;
            case 0x48: // ret
            case 0x49:
                if (code != Code[^1]) throw new Exception();
                break;
            case 0x4a: // send
            case 0x4b:
                {
                    var cnt = (byte)code.Arguments[0] / 2;
                    var target = GetAcc();
                    target.Use();
                    if (target.Var != null)
                        target = target.Var;

                    var frame = PopFrame(cnt);
                    for (int i = 0; i < cnt; i++)
                    {
                        var selector = frame[i];
                        var argsExp = frame[++i];
                        var argsCnt = argsExp.GetValue();

                        List<Expr> args = new(argsCnt);
                        for (int j = 0; j < argsCnt; j++)
                            args.Add(frame[++i]);

                        if (selector is ConstExpr c)
                        {
                            SetAcc(new CallExpr(target, GetName(c.Value), args));
                        }
                        else
                        {
                            // TODO переделать
                            SetAcc(new CallExpr(target, selector.Label, args));
                        }
                    }
                    _rest = 0;
                }
                break;
            case 0x050: // class
            case 0x051:
                {
                    var id = GetVal(code.Arguments[0]);
                    var cl = Procedure.GetClass(id);
                    if (cl != null)
                        SetAcc(new ClassExpr(cl));
                    else
                        SetAcc(new ClassExpr($"Class_{id}"));
                }
                break;
            case 0x54: // self
            case 0x55:
                {
                    var cnt = (byte)code.Arguments[0] / 2;
                    var frame = PopFrame(cnt);
                    for (int i = 0; i < cnt; i++)
                    {
                        var sel = frame[i].GetValue();
                        var name = GetName(sel);
                        bool isProp = false;
                        if (Procedure.Class == null)
                            name = "this." + name;
                        else
                            isProp = Procedure.Class.IsProp(sel);
                        var argsExp = frame[++i];
                        var argsCnt = argsExp.GetValue();

                        if (argsCnt > 0 || _rest > 0)
                        {
                            List<Expr> args = new(argsCnt + _rest);
                            for (int j = 0; j < argsCnt; j++)
                                args.Add(frame[++i]);

                            if (_rest > 0) // TODO Calc params count
                                args.Add(Procedure.GetParam(1));

                            if (isProp)
                            {
                                if (args.Count != 1) throw new InvalidOperationException();
                                Set(new ParamExpr(name), args[0], true);
                            }
                            else
                                AddExpr(new CallExpr(name, args));
                        }
                        else
                        {
                            if (isProp)
                                SetAcc(new ParamExpr(name));
                            else
                                SetAcc(new CallExpr(name));
                        }
                    }
                    _rest = 0;
                }
                break;
            case 0x56: // super
            case 0x57:
                {
                    var sel = GetVal(code.Arguments[0]);
                    var super = _package.GetClassSection(sel);
                    var className = Expr.ToCppName(super);
                    var cnt = (byte)code.Arguments[1] / 2;

                    var frame = PopFrame(cnt);
                    for (int i = 0; i < cnt; i++)
                    {
                        var selector = frame[i];
                        var name = className + "::" + GetName(selector.GetValue());
                        var argsExp = frame[++i];
                        var argsCnt = argsExp.GetValue();

                        if (argsCnt > 0 || _rest > 0)
                        {
                            List<Expr> args = new(argsCnt + _rest);
                            for (int j = 0; j < argsCnt; j++)
                                args.Add(frame[++i]);

                            if (_rest > 0) // TODO Calc params count
                                args.Add(Procedure.GetParam(1));

                            SetAcc(new CallExpr(name, args));
                        }
                        else
                        {
                            SetAcc(new CallExpr(name));
                        }
                    }
                    _rest = 0;
                }
                break;
            case 0x58: // &rest
            case 0x59:
                _rest = GetVal(code.Arguments[0]);
                //if (_rest != 1) Console.WriteLine(code);
                break;
            case 0x5a: // lea
            case 0x5b:
                {
                    var t = GetVal(code.Arguments[0]);
                    var ind = GetVal(code.Arguments[1]);

                    var type = t >> 1 & 3;
                    var param = OpGetExpression(type, ind);
                    Expr ex = param;
                    if ((t & 0x10) > 0) // Use acc
                        ex = new ArrayExpr(param, GetAcc());
                    SetAcc(new Math1Expr(ex, "&"));
                }
                break;
            case 0x5c: // selfID
            case 0x5d:
                SetAcc(new ParamExpr("this"));
                break;
            case 0x60: // pprev
            case 0x61:
                Push(GetPrev());
                break;
            case 0x62: // pToa
            case 0x63:
                SetAcc(GetPropExpr(code.Arguments[0]));
                break;
            case 0x64: // aTop
            case 0x65:
                Set(GetPropExpr(code.Arguments[0]), GetAcc(), true);
                break;
            case 0x66: // pTos
            case 0x67:
                Push(GetPropExpr(code.Arguments[0]));
                break;
            case 0x68: // sTop
            case 0x69:
                Set(GetPropExpr(code.Arguments[0]), Pop(), true);
                break;
            case 0x6a: // ipToa
            case 0x6b:
                {
                    var prop = GetPropExpr(code.Arguments[0]);
                    Set(prop, new Math2Expr(prop, "+", new ConstExpr(1)), true);
                    SetAcc(prop);
                }
                break;
            case 0x6c: // dpToa
            case 0x6d:
                {
                    var prop = GetPropExpr(code.Arguments[0]);
                    Set(prop, new Math2Expr(prop, "-", new ConstExpr(1)), true);
                    SetAcc(prop);
                }
                break;
            case 0x6e: // ipTos
            case 0x6f:
                {
                    var prop = GetPropExpr(code.Arguments[0]);
                    Set(prop, new Math2Expr(prop, "+", new ConstExpr(1)), true);
                    Push(prop);
                }
                break;
            case 0x70: // dpTos
            case 0x71:
                {
                    var prop = GetPropExpr(code.Arguments[0]);
                    Set(prop, new Math2Expr(prop, "-", new ConstExpr(1)), true);
                    Push(prop);
                }
                break;
            case 0x72: // lofsa
            case 0x73:
                SetAcc((RefToElement)code.Arguments[0]);
                break;
            case 0x74: // lofss
            case 0x75:
                Push(GetExpr((RefToElement)code.Arguments[0]));
                break;
            case 0x76: // push0
            case 0x77:
                Push(0);
                break;
            case 0x78: // push1
            case 0x79:
                Push(1);
                break;
            case 0x7a: // push2
            case 0x7b:
                Push(2);
                break;
            case 0x7c: // pushSelf
            case 0x7d:
                Push(new ParamExpr("this"));
                break;

            default:
                Console.WriteLine($"// Instruction not implemented {code.Name} {code.Type:x02}");
                //throw new NotImplementedException();
                break;
        }
    }

    private string GetName(ushort sel) => Expr.ToCppName(_package.GetName(sel));

    private ParamExpr OpGetExpression(int type, ushort ind)
    {
        switch (type)
        {
            case 0: // Global
                _mainUsed = true;
                return new ParamExpr($"global{ind}");
            case 1: // Local
                return new ParamExpr($"local{ind}");
            case 2: // Temp
                return new ParamExpr($"tmp{ind}");
            case 3: // Parameter
                return Procedure.GetParam(ind);
            default:
                Console.WriteLine($"// Op type {type} not implemented");
                return null;
        }
    }

    private ParamExpr GetPropExpr(object val)
    {
        var ind = GetVal(val) / 2;
        var cl = Procedure.Class;
        if (cl == null)
        {
            Console.WriteLine("No class method");
            return new ParamExpr($"prop{ind}");
        }
        if (ind >= cl.Properties.Length)
            return new ParamExpr($"prop{ind}");
        return new ParamExpr(cl.Properties[ind]);
    }

    private static Expr GetExpr(RefToElement r)
    {
        if (r.Reference is PropertyElement prop)
        {
            if (prop.Index == 0)
                return new ParamExpr(prop.Class);
        }
        return new RefExpr(r);
    }

    private static ushort GetVal(object val)
    {
        if (val is byte b) return b;
        if (val is ushort s) return s;
        throw new NotImplementedException();
    }

    private void Push(ushort val) => Push(new ConstExpr(val));

    private void Push(Expr exp)
    {
        _stack.Push(exp);
    }

    private Expr TOS()
    {
        if (_stack.Any())
            return _stack.Peek();

        if (_prevStack.Any())
            return _prevStack.Peek();

        throw new InvalidOperationException($"{this} PEEK EMPTY STACK!!!");
    }

    private Expr Pop()
    {
        if (_stack.Any())
            return _stack.Pop();

        if (_prevStack.Any())
            return _prevStack.Pop();

        throw new InvalidOperationException($"{this} POP EMPTY STACK!!!");
    }


    private List<Expr> PopArgs(int cnt)
    {
        // TODO Rest check
        List<Expr> args = new();
        for (int i = 0; i < cnt; i++)
            args.Add(Pop());
        args.Reverse();
        if (_rest > 0)
            args.Add(new ParamExpr($"&rest{_rest}"));

        var pcnt = Pop(); // Params cnt
        _rest = 0;
        return args;
    }

    private Expr[] PopFrame(int cnt)
    {
        Expr[] frame = new Expr[cnt];
        for (int i = 0; i < cnt; i++)
            frame[cnt - i - 1] = Pop();
        return frame;
    }


    private void AddExpr(Expr exp)
    {
        Expressions.Add(exp);
        exp.Block = this;
    }
    private void Set(Expr param, Expr value, bool ext) => AddExpr(new SetExpr(param, value, ext));


    private void SetAcc(ushort val) => SetAcc(new ConstExpr(val));
    private void SetAcc(RefToElement r) => SetAcc(GetExpr(r));
    private void SetAcc(CallExpr expr)
    {
        AddExpr(expr);
        Acc = expr;
    }
    private void SetAcc(Expr expr)
    {
        Acc = expr;
    }
    private Expr GetAcc()
    {
        if (Acc == null) throw new Exception();
        return Acc;
    }


    private Expr GetPrev()
    {
        if (Prev == null) throw new Exception();
        return Prev;
    }
}
