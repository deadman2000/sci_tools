using System;
using System.Text;
using static SCI_Lib.Resources.Vocab.Parser;

namespace SCI_Lib.Resources.Vocab;

public class ParseTreeNode
{
    public ParseTypes Type { get; set; }
    public ParseTreeNode Left { get; set; }
    public ParseTreeNode Right { get; set; }
    public ushort Value { get; set; }

    public ushort Minor => Right.Left.Value;

    public ushort Major => Left.Value;

    public bool IsTerminal => Right.Right.Type != ParseTypes.Branch;

    public ushort TerminalValue => Right.Right.Value;

    internal int WriteSubexpression(ParseRule rule, int rulePos)
    {
        ParseTreeNode node = this;
        while (true)
        {
            var token = (rulePos < rule.Data.Count) ? rule.Data[rulePos++] : TOKEN_CPAREN;
            if (token == TOKEN_CPAREN) break;

            var nextToken = (rulePos < rule.Data.Count) ? rule.Data[rulePos] : TOKEN_CPAREN;
            var val = (ushort)(token & 0xffff);
            if (token == TOKEN_OPAREN)
            {
                var next = node.Pareno();
                rulePos = next.WriteSubexpression(rule, rulePos);
                nextToken = (rulePos < rule.Data.Count) ? rule.Data[rulePos] : TOKEN_CPAREN;
                if (nextToken != TOKEN_CPAREN)
                    node = node.Parenc();
            }
            else if ((token & TOKEN_STUFFING_LEAF) != 0)
            {
                if (nextToken == TOKEN_CPAREN)
                    node.Terminate(val);
                else
                    node = node.Append(val);
            }
            else if ((token & TOKEN_STUFFING_WORD) != 0)
            {
                if (nextToken == TOKEN_CPAREN)
                    node.TerminateWord(val);
                else
                    node = node.AppendWord(val);
            }
            else
                throw new Exception("VbptWriteSubexpression error");
        }

        return rulePos;
    }

    internal ParseTreeNode Pareno()
    {
        Left = new ParseTreeNode
        {
            Type = ParseTypes.Branch,
        };
        return Left;
    }

    internal ParseTreeNode Parenc()
    {
        Right = new ParseTreeNode
        {
            Type = ParseTypes.Branch,
        };
        return Right;
    }

    internal ParseTreeNode Append(ushort value)
    {
        Left = new ParseTreeNode
        {
            Type = ParseTypes.Leaf,
            Value = value,
        };
        Right = new ParseTreeNode
        {
            Type = ParseTypes.Branch
        };
        return Right;
    }

    internal ParseTreeNode AppendWord(ushort value)
    {
        Type = ParseTypes.Word;
        Value = value;
        Right = new ParseTreeNode
        {
            Type = ParseTypes.Branch
        };
        return Right;
    }

    internal void Terminate(ushort value)
    {
        Type = ParseTypes.Leaf;
        Value = value;
    }

    internal void TerminateWord(ushort value)
    {
        Type = ParseTypes.Word;
        Value = value;
    }

    public string GetTree(string name)
    {
        StringBuilder sb = new();
        sb.AppendLine($"(setq {name}");
        sb.Append("'(");
        VocabRecursivePtreeDump(sb, 1);
        sb.AppendLine("))");
        return sb.ToString();
    }

    private void VocabRecursivePtreeDump(StringBuilder sb, int blanks)
    {
        if (Type == ParseTypes.Leaf)
        {
            sb.Append("VocabRecursivePtreeDump: Error: consp is nil");
            return;
        }

        if (Left != null)
        {
            if (Left.Type == ParseTypes.Branch)
            {
                sb.AppendLine();
                WriteSpace(sb, blanks);
                sb.Append('(');
                Left.VocabRecursivePtreeDump(sb, blanks + 1);
                sb.AppendLine(")");
                WriteSpace(sb, blanks);
            }
            else
                sb.Append($"{Left.Value:x}");
            sb.Append(' ');
        }

        if (Right != null)
        {
            if (Right.Type == ParseTypes.Branch)
                Right.VocabRecursivePtreeDump(sb, blanks);
            else
            {
                sb.Append($"{Right.Value:x}");
                var node = Right;
                while (node.Right != null)
                {
                    node = node.Right;
                    sb.Append($"/{node.Value:x}");
                }
            }
        }
    }

    private static void WriteSpace(StringBuilder sb, int blanks)
    {
        for (int i = 0; i < blanks; i++)
            sb.Append("    ");
    }

    public void AttachSubtree(ushort major, ushort minor, ParseTreeNode subtree)
    {
        AttachRight(
            CreateBranch(
                CreateBranch(
                    CreateLeaf(major),
                    subtree.AttachLeft(CreateLeaf(minor))
                ),
                null)
        );
    }

    public ParseTreeNode AttachRight(ParseTreeNode right)
    {
        Type = ParseTypes.Branch;
        Right = right;
        return this;
    }

    public ParseTreeNode AttachLeft(ParseTreeNode left)
    {
        Type = ParseTypes.Branch;
        Left = left;
        return this;
    }

    public static ParseTreeNode CreateBranch(ParseTreeNode left, ParseTreeNode right) => new()
    {
        Type = ParseTypes.Branch,
        Left = left,
        Right = right,
    };

    public static ParseTreeNode CreateLeaf(ushort value) => new()
    {
        Type = ParseTypes.Leaf,
        Value = value
    };

    public static ParseTreeNode CreateWord(ushort value) => new()
    {
        Type = ParseTypes.Word,
        Value = value
    };
}

public enum ParseTypes
{
    Word,
    Leaf,
    Branch
}
