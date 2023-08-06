using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Resources.Scripts.Analyzer;

public class ProcedureTree
{
    private readonly SCIPackage _package;

    private readonly HashSet<ushort> _usedScripts = new();
    private readonly Dictionary<ushort, CodeBlock> _blocksByAddress = new();
    private readonly Dictionary<ushort, ParamExpr> _params = new();
    private readonly List<LinkExpr> _links = new();
    private int _nextVarId = 0;
    private CodeBlock _first;
    private OptimizedTree _opt;

    public Code Begin { get; }
    public Code End { get; private set; }
    public string Name { get; }

    public ScriptAnalyzer Decompiler { get; }
    public ClassSection Class { get; }

    public IEnumerable<CodeBlock> Blocks => _blocksByAddress.Values;
    public IEnumerable<ParamExpr> Params => _params.Values;

    public string Define
    {
        get
        {
            if (_params.Count == 0)
                return $"{Name}()";

            var args = string.Join(", ", _params.OrderBy(kv => kv.Key)
                .Select(kv => kv.Value)
                .Select(p => $"{p.Type} {p.Name}")
                .ToArray()
            );
            return $"{Name}({args})";
        }
    }

    public ProcedureTree(ScriptAnalyzer decompiler, ClassSection cl, Code code, string name)
    {
        _package = code.Script.Package;
        Decompiler = decompiler;
        Class = cl;
        Begin = code;
        Name = Expr.ToCppName(name);
    }

    public override string ToString()
    {
        string str = "";
        if (Class != null)
        {
            if (Class.Name != null)
                str = Expr.ToCppName(Class.Name);
            else
                str = $"{Class.Id:x4}";
            str += "::";
        }
        str += Define;
        return str;
    }

    private void Clear()
    {
        _first = null;
        _blocksByAddress.Clear();
        _links.Clear();
        _nextVarId = 0;
    }

    public void BuildMethod()
    {
        if (_first != null) Clear();

        var code = Begin;
        HashSet<Code> additionalBlocks = new();
        _params.Clear();

        while (true)
        {
            ushort lastJump = 0;
            List<Code> jupms = new();
            List<Code> instructions = new();
            var sectorBegin = code;

            while (true)
            {
                End = code;
                instructions.Add(code);
                if (code.IsReturn)
                {
                    if (lastJump == 0 || code.Address >= lastJump)
                        break;
                }

                switch (code.Type)
                {
                    case 0x2e: // bt
                    case 0x2f:
                    case 0x30: // bnt
                    case 0x31:
                    case 0x32: // jmp
                    case 0x33:
                        jupms.Add(code.Next);
                        var r = code.Arguments[0] as CodeRef;
                        if (r.Reference is Code rc)
                        {
                            var addr = rc.Address;
                            if (addr < Begin.Address)
                                additionalBlocks.Add(rc);
                            else
                            {
                                lastJump = Math.Max(lastJump, addr);
                                jupms.Add(rc);
                            }
                        }
                        break;
                    case 0x48: // ret
                    case 0x49:
                        if (code.Next != null)
                            jupms.Add(code.Next);
                        break;
                }

                code = code.Next;
                if (code == null) break;
            }

            if (jupms.Count > 0)
            {
                var outCode = jupms.Where(c => c.Address > End.Address); // Код из другой секции
                foreach (var c in outCode) additionalBlocks.Add(c);

                var address = jupms.Where(c => c.Address <= End.Address)
                    .Distinct()
                    .Where(c => c != Begin)
                    .Select(c => c.Address)
                    .OrderBy(a => a)
                    .ToArray();

                var bl = CreateBlock(instructions.Where(c => c.Address < address[0]), _first == null);
                _first ??= bl;
                for (int i = 0; i < address.Length - 1; i++)
                    CreateBlock(instructions.Where(c => c.Address >= address[i] && c.Address < address[i + 1]));
                CreateBlock(instructions.Where(c => c.Address >= address[^1]));
            }
            else
            {
                var bl = CreateBlock(instructions, _first == null);
                _first ??= bl;
            }

            additionalBlocks.RemoveWhere(c => c.Address >= sectorBegin.Address && c.Address <= End.Address);
            if (!additionalBlocks.Any())
                break;
            code = additionalBlocks.OrderBy(c => c.Address).First();
        }


        foreach (var block in _blocksByAddress.Values)
            block.BuildTree();

        _first.Decompile();

        CalcLinks();
    }

    private void CalcLinks()
    {
        while (true)
        {
            foreach (var link in _links)
                CheckUsed(link);
            if (!_links.Any(l => l.Used && !l.Checked))
                break;
        }
    }

    private void CheckUsed(LinkExpr link)
    {
        if (!link.Used) return;

        if (link.Checked) return;
        link.Checked = true;

        foreach (var (_, e) in link.Links)
        {
            e.Use();
            if (e is LinkExpr l)
                CheckUsed(l);
        }

        if (link.Links.Count > 1)
        {
            var v = CreateVar();
            foreach (var (b, e) in link.Links)
            {
                var val = link.IsAcc ? b.Acc : b.Prev;
                if (e.Var != null)
                {
                    b.Expressions.Add(new SetExpr(v, e.Var, false));
                }
                else
                {
                    b.Expressions.Add(new SetExpr(v, val, false));
                    e.MakeVar(v);
                }
            }
            link.SetVar(v);
        }
    }

    private CodeBlock CreateBlock(IEnumerable<Code> ops, bool begin = false)
    {
        if (!ops.Any()) throw new ArgumentException("Empty collection", nameof(ops));

        var block = new CodeBlock(this, ops.ToList(), begin);
        _blocksByAddress.Add(block.AddrBegin, block);
        return block;
    }

    internal string GetVarName() => $"var{_nextVarId++}";
    internal ParamExpr CreateVar() => new(GetVarName());

    internal ParamExpr GetParam(ushort num)
    {
        if (_params.TryGetValue(num, out var e)) return e;
        var name = num == 0 ? "argc" : $"p{num}";
        e = new ParamExpr(name);
        _params.Add(num, e);
        return e;
    }

    internal CodeBlock GetBlock(Code code)
    {
        if (_blocksByAddress.TryGetValue(code.Address, out var block)) return block;
        throw new InvalidOperationException();
    }

    internal void RegisterLink(LinkExpr link)
    {
        _links.Add(link);
    }

    internal ClassSection GetClass(ushort id)
    {
        var cl = _package.GetClass(id);
        if (cl != null) Using(cl.Script.Resource);
        return cl;
    }

    internal void Using(Resource resource) => _usedScripts.Add(resource.Number);
    internal void Using(ushort scr) => _usedScripts.Add(scr);

    public OptimizedTree Optimize()
    {
        if (_opt != null) return _opt;
        _opt = new OptimizedTree(_first);
        _opt.Optimize();
        Clear();
        return _opt;
    }
}