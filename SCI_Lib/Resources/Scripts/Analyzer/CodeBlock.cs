using SCI_Lib.Resources.Scripts.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCI_Lib.Resources.Scripts.Analyzer;

public class CodeBlock
{
    private readonly ProcedureTree _proc;
    private readonly SCIPackage _package;

    public List<Expr> Expressions { get; } = new();
    private readonly List<Expr> _stack = new();

    private bool _isBuild;
    private bool _isDecompiled;
    private bool _isLinkCalc;

    // Ссылки для сбора значений аккумуляторов, которые могут прийти из других блоков
    private readonly LinkExpr _linkAcc;
    private readonly LinkExpr _linkPrev;

    private Expr _prev;
    private Expr _acc;
    private Expr _condition;
    private ushort _rest;

    public List<Code> Code { get; }
    public List<CodeBlock> Parents { get; } = new();

    public Code NextA { get; set; }
    public CodeBlock BlockA { get; set; }

    public Code NextB { get; set; }
    public CodeBlock BlockB { get; set; }

    public bool IsBegin { get; set; }

    public ushort AddrBegin => Code[0].Address;
    public ushort AddrEnd => Code[^1].Address;
    public bool ReturnA => NextA != null && NextA.IsReturn;
    public bool ReturnB => NextB != null && NextB.IsReturn;

    public CodeBlock(ProcedureTree proc, List<Code> list)
    {
        _proc = proc;
        _package = list[0].Script.Package;
        _linkAcc = new LinkExpr();
        _linkPrev = new LinkExpr();
        _acc = _linkAcc;
        _prev = _linkPrev;
        Code = list;
    }

    public string ASM => $"{AddrBegin:x04}:{AddrEnd:x04}\\n" + string.Join("\\l", Code.Select(c => Escape(c.ASM.Trim()))) + "\\l";

    public string GetCppGraph()
    {
        StringBuilder code = new();
        foreach (var exp in Expressions)
            code.Append(exp.Label).Append("\\l");

        if (_condition != null)
            code.Append($"if ({_condition.VarLabel})\\l");

        return Escape(code.ToString());
    }

    public string GetMetaGraph()
    {
        StringBuilder code = new($"{AddrBegin:x04}:{AddrEnd:x04}\\n");
        foreach (var exp in Expressions)
            code.Append(exp).Append($" ->{exp.LinksCount}").Append("\\l");

        if (_condition != null)
            code.Append($"if ({_condition})\\l");

        return Escape(code.ToString());
    }

    private static string Escape(string str) => str.Replace("\"", "'");

    public string Label => $"Code{AddrBegin:x4}";
    public override string ToString() => $"{AddrBegin:x4} -> ({NextA?.Address:x4}, {NextB?.Address:x4})";



    public void BuildTree()
    {
        if (_isBuild) return;

        CalcNext();
        _isBuild = true;

        if (NextA != null && !NextA.IsReturn)
        {
            BlockA = _proc.GetBlock(NextA);
            BlockA.Parents.Add(this);
            BlockA.BuildTree();
        }
        if (NextB != null && !NextB.IsReturn)
        {
            BlockB = _proc.GetBlock(NextB);
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

        if (last.Type == 0x32) // jmp
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


    public void Decompile()
    {
        if (_isDecompiled) return;

        foreach (var c in Code)
            ExecuteCode(c);

        if (_condition != null)
            Expressions.Remove(_condition);

        Expressions.RemoveAll(e => e.LinksCount == 1);

        _isDecompiled = true;

        if (BlockA != null)
        {
            if (!BlockA._isDecompiled)
            {
                BlockA._stack.AddRange(_stack);
                BlockA.Decompile();
            }

            if (_condition != null)
                BlockB.LinkTo(this, true);
            else
                BlockA.LinkTo(this);
        }

        if (BlockB != null)
        {
            if (!BlockB._isDecompiled)
            {
                BlockB._stack.AddRange(_stack);
                BlockB.Decompile();
            }

            if (_condition != null)
                BlockB.LinkTo(this, false);
            else
                BlockB.LinkTo(this);
        }
    }

    private void LinkTo(CodeBlock block, bool trueBranch)
    {
        _linkAcc.LinkTo(block, new UShortConst(trueBranch ? 1 : 0));
        _linkPrev.LinkTo(block, block._prev);
    }

    private void LinkTo(CodeBlock block)
    {
        _linkAcc.LinkTo(block, block._acc);
        _linkPrev.LinkTo(block, block._prev);
    }

    public void CalcAcc()
    {
        if (_isLinkCalc) return;
        _isLinkCalc = true;

        if (_linkAcc.Used && _linkAcc.Links.Count > 1)
        {
            var v = _proc.CreateVar();
            _linkAcc.SetVar(v);
            foreach (var (Block, Expression) in _linkAcc.Links)
            {
                Expression.MakeVar(v.Name);
                Block.Expressions.Remove(Block._acc);
                Block.Set(v, Block._acc);
            }
        }

        if (_linkPrev.Used && _linkPrev.Links.Count > 1)
        {
            var v = _proc.CreateVar();
            _linkPrev.SetVar(v);
            foreach (var (Block, Expression) in _linkPrev.Links)
            {
                Expression.MakeVar(v.Name);
                Block.Expressions.Remove(Block._prev);
                Block.Set(v, Block._prev);
            }
        }

        BlockA?.CalcAcc();
        BlockB?.CalcAcc();
    }

    private void ExecuteCode(Code code)
    {
        if (code.Type >= 0x80)
        {
            ushort val = GetVal(code.Arguments[0]);
            var type = code.Type >> 1 & 0x3;
            bool isStack = (code.Type & 1 << 3) != 0;
            bool isAccIndex = (code.Type & 1 << 4) != 0;

            Expr varialbe = OpGetExpression(type, val);
            if (isAccIndex)
                varialbe = new ArrayExpr((ParamExpr)varialbe, GetAcc());

            var exec = code.Type >> 5 & 0x3;

            if (exec == 2) // Increment
                varialbe = new Math1Expr(varialbe, "++");
            else if (exec == 3) // Decrement
                varialbe = new Math1Expr(varialbe, "--");

            switch (exec)
            {
                case 0: // Load
                case 2: // Increment
                case 3: // Decrement
                    if (isStack)
                        _stack.Add(varialbe);
                    else
                        SetAcc(varialbe);
                    break;
                case 1: // Store
                    {
                        Expr op;
                        if (isStack)
                            op = Pop();
                        else
                            op = GetAcc();
                        Set(varialbe, op);
                    }
                    break;
            }
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
                _prev = GetAcc();
                SetAcc(new Math2Expr(Pop(), "==", GetAcc()));
                break;
            case 0x1c: // ne?
            case 0x1d:
                _prev = GetAcc();
                SetAcc(new Math2Expr(Pop(), "!=", GetAcc()));
                break;
            case 0x1e: // gt?
            case 0x1f:
                _prev = GetAcc();
                SetAcc(new Math2Expr(Pop(), ">", GetAcc()));
                break;
            case 0x20: // ge?
            case 0x21:
                _prev = GetAcc();
                SetAcc(new Math2Expr(Pop(), ">=", GetAcc()));
                break;
            case 0x22: // lt?
            case 0x23:
                _prev = GetAcc();
                SetAcc(new Math2Expr(Pop(), "<", GetAcc()));
                break;
            case 0x24: // le?
            case 0x25:
                _prev = GetAcc();
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
                _condition = GetAcc();
                _condition.Use();
                break;
            case 0x32: //jmp
            case 0x33:
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
                        Push(_proc.GetParam(i));
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
                    ushort ind = GetVal(code.Arguments[0]);
                    var cnt = (byte)code.Arguments[1] / 2;
                    var args = PopArgs(cnt);
                    SetAcc(new CallExpr($"proc_{ind}", args));
                }
                break;
            case 0x46: // calle
            case 0x47:
                {
                    ushort scr = GetVal(code.Arguments[0]);
                    var ind = GetVal(code.Arguments[1]);
                    var cnt = (byte)code.Arguments[2] / 2;
                    var args = PopArgs(cnt);
                    SetAcc(new CallExpr($"scr{scr}_{ind:x2}", args));
                }
                break;
            case 0x48: // ret
            case 0x49:
                break;
            case 0x4a: // send
            case 0x4b:
                {
                    var cnt = (byte)code.Arguments[0] / 2;
                    var target = GetAcc();
                    if (target is CallExpr)
                    {
                        var varExp = _proc.CreateVar();
                        Expressions.Remove(target);
                        Set(varExp, target);
                        target = varExp;
                    }

                    var frame = PopFrame(cnt);
                    for (int i = 0; i < cnt; i++)
                    {
                        var selector = frame[i];
                        string method;

                        var argsExp = frame[++i];
                        var argsCnt = argsExp.GetValue();

                        List<Expr> args = new(argsCnt);
                        for (int j = 0; j < argsCnt; j++)
                            args.Add(frame[++i]);

                        if (selector is UShortConst)
                        {
                            method = _package.GetName(selector.GetValue());
                            SetAcc(new CallExpr(target, method, args));
                        }
                        else
                        {
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
                    var cl = _package.GetClass(id);
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
                        var selector = frame[i];
                        var name = _package.GetName(selector.GetValue());
                        var argsExp = frame[++i];
                        var argsCnt = argsExp.GetValue();
                        var isProp = _proc.Class.IsProp(name);

                        if (argsCnt > 0 || _rest > 0)
                        {
                            List<Expr> args = new(argsCnt + _rest);
                            for (int j = 0; j < argsCnt; j++)
                                args.Add(frame[++i]);

                            if (_rest > 0) // TODO Calc params count
                                args.Add(_proc.GetParam(1));

                            if (isProp)
                            {
                                if (args.Count != 1) throw new InvalidOperationException();
                                Set(new ParamExpr(name), args[0]);
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
                    var super = _package.GetClassSection(GetVal(code.Arguments[0]));
                    var className = super.Name;
                    var cnt = (byte)code.Arguments[1] / 2;

                    var frame = PopFrame(cnt);
                    for (int i = 0; i < cnt; i++)
                    {
                        var selector = frame[i];
                        var name = className + "::" + _package.GetName(selector.GetValue());
                        var argsExp = frame[++i];
                        var argsCnt = argsExp.GetValue();

                        if (argsCnt > 0 || _rest > 0)
                        {
                            List<Expr> args = new(argsCnt + _rest);
                            for (int j = 0; j < argsCnt; j++)
                                args.Add(frame[++i]);

                            if (_rest > 0) // TODO Calc params count
                                args.Add(_proc.GetParam(1));

                            AddExpr(new CallExpr(name, args));
                        }
                        else
                        {
                            SetAcc(new CallExpr(name));
                        }
                    }
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
                Set(GetPropExpr(code.Arguments[0]), GetAcc());
                break;
            case 0x66: // pTos
            case 0x67:
                Push(GetPropExpr(code.Arguments[0]));
                break;
            case 0x68: // sTop
            case 0x69:
                Set(GetPropExpr(code.Arguments[0]), Pop());
                break;
            case 0x6a: // ipToa
            case 0x6b:
                SetAcc(new Math1Expr(GetPropExpr(code.Arguments[0]), "++"));
                break;
            case 0x6c: // dpToa
            case 0x6d:
                SetAcc(new Math1Expr(GetPropExpr(code.Arguments[0]), "--"));
                break;
            case 0x6e: // ipTos
            case 0x6f:
                Push(new Math1Expr(GetPropExpr(code.Arguments[0]), "++"));
                break;
            case 0x70: // dpTos
            case 0x71:
                Push(new Math1Expr(GetPropExpr(code.Arguments[0]), "--"));
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

    private ParamExpr OpGetExpression(int type, ushort ind)
    {
        switch (type)
        {
            case 0: // Global
                return new ParamExpr($"global{ind}");
            case 1: // Local
                return new ParamExpr($"local{ind}");
            case 2: // Temp
                return new ParamExpr($"tmp{ind}");
            case 3: // Parameter
                return _proc.GetParam(ind);
            default:
                Console.WriteLine($"// Op type {type} not implemented");
                return null;
        }
    }

    private void MakeVar(Expr expr)
    {
        if (expr.IsSimple) return; // для UShort и Param не нужны переменные, используем их как есть. для Link мы и так создаём переменную
        var v = _proc.CreateVar();
        expr.MakeVar(v.Name);
        Set(v, expr);
        Expressions.Remove(expr);
    }

    private ParamExpr GetPropExpr(object val)
    {
        var cl = _proc.Class;
        return new ParamExpr(cl.Properties[GetVal(val) / 2].Name);
    }

    private static Expr GetExpr(RefToElement r)
    {
        if (r.Reference is PropertyElement prop)
        {
            if (prop.Index == 0)
                return new ParamExpr(prop.Class.Name);
        }
        return new RefExpr(r);
    }

    private static ushort GetVal(object val)
    {
        if (val is byte b) return b;
        if (val is ushort s) return s;
        throw new NotImplementedException();
    }

    private void Push(ushort val) => Push(new UShortConst(val));

    private void Push(Expr exp)
    {
        AddExpr(exp);
        _stack.Add(exp);
    }

    private Expr TOS() => _stack[^1];

    private Expr Pop()
    {
        if (_stack.Count == 0) throw new Exception("Stack empty");
        var val = _stack.Last();
        _stack.RemoveAt(_stack.Count - 1);
        return val;
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

    private List<Expr> PopFrame(int cnt)
    {
        List<Expr> frame = new();
        for (int i = 0; i < cnt; i++)
            frame.Add(Pop());
        frame.Reverse();
        return frame;
    }


    private void AddExpr(Expr exp)
    {
        if (exp.IsSimple) return;
        Expressions.Add(exp);
    }
    private void Set(Expr param, Expr value) => AddExpr(new SetExpr(param, value));


    private void SetAcc(ushort val) => SetAcc(new UShortConst(val));
    private void SetAcc(RefToElement r) => SetAcc(GetExpr(r));
    private void SetAcc(Expr expr)
    {
        AddExpr(expr);
        _acc = expr;
    }
    private Expr GetAcc()
    {
        if (_acc == null) throw new Exception();
        return _acc;
    }


    private Expr GetPrev()
    {
        if (_prev == null) throw new Exception();
        return _prev;
    }
}
