using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Resources.Scripts.Analyzer;

public class ProcedureTree
{
    private readonly Code _begin;

    private readonly Dictionary<ushort, CodeBlock> _blocksByAddress = new();
    private readonly Dictionary<ushort, ParamExpr> _params = new();

    private int _nextVarId = 0;

    public string Name { get; }
    public ScriptAnalyzer Decompiler { get; }
    public ClassSection Class { get; }

    public IEnumerable<CodeBlock> Blocks => _blocksByAddress.Values;


    public ProcedureTree(ScriptAnalyzer decompiler, ClassSection cl, Code code, string name)
    {
        Decompiler = decompiler;
        Class = cl;
        _begin = code;
        Name = name;
    }

    public void BuildMethod()
    {
        var code = _begin;
        ushort end = 0;
        List<Code> jupms = new();
        List<Code> instructions = new();
        while (true)
        {
            instructions.Add(code);
            if (code.IsReturn)
            {
                if (end == 0 || code.Address >= end)
                    break;
            }
            else
            {
                switch (code.Type)
                {
                    case 0x32: // jmp
                    case 0x2e: // bt
                    case 0x2f:
                    case 0x30: // bnt
                    case 0x31:
                        var r = code.Arguments[0] as CodeRef;
                        var rc = (Code)r.Reference;
                        var addr = rc.Address;
                        end = Math.Max(end, addr);
                        jupms.Add(code.Next);
                        jupms.Add(rc);
                        break;
                }
            }

            code = code.Next;
            if (code == null) break;
        }

        CodeBlock first;
        if (jupms.Count > 0)
        {
            var address = jupms.Distinct()
                .Where(c => c != _begin)
                .Select(c => c.Address)
                .OrderBy(a => a)
                .ToArray();

            first = CreateBlock(instructions.Where(c => c.Address < address[0]));
            for (int i = 0; i < address.Length - 1; i++)
                CreateBlock(instructions.Where(c => c.Address >= address[i] && c.Address < address[i + 1]));
            CreateBlock(instructions.Where(c => c.Address >= address[^1]));
        }
        else
        {
            first = CreateBlock(instructions);
        }

        foreach (var block in _blocksByAddress.Values)
            block.BuildTree();

        first.IsBegin = true;
        first.BuildTree();
        first.Decompile();
        first.CalcAcc();
    }

    private CodeBlock CreateBlock(IEnumerable<Code> ops)
    {
        var block = new CodeBlock(this, ops.ToList());
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
}
