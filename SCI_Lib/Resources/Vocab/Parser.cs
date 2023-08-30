using SCI_Lib.Resources.Scripts.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SCI_Lib.Resources.Vocab;

public class Parser
{
    public static readonly uint TOKEN_OPAREN = 0xff000000;
    public static readonly uint TOKEN_CPAREN = 0xfe000000;
    public static readonly uint TOKEN_TERMINAL_CLASS = 0x10000;
    public static readonly uint TOKEN_TERMINAL_GROUP = 0x20000;
    public static readonly uint TOKEN_STUFFING_LEAF = 0x40000;
    public static readonly uint TOKEN_STUFFING_WORD = 0x80000;
    public static readonly uint TOKEN_NON_NT = (TOKEN_OPAREN | TOKEN_TERMINAL_CLASS | TOKEN_TERMINAL_GROUP | TOKEN_STUFFING_LEAF | TOKEN_STUFFING_WORD);
    public static readonly uint TOKEN_TERMINAL = (TOKEN_TERMINAL_CLASS | TOKEN_TERMINAL_GROUP);

    const ushort VOCAB_TREE_NODE_LAST_WORD_STORAGE = 0x140;
    const ushort VOCAB_TREE_NODE_COMPARE_TYPE = 0x146;
    const ushort VOCAB_TREE_NODE_COMPARE_GROUP = 0x14d;
    const ushort VOCAB_TREE_NODE_FORCE_STORAGE = 0x154;

    const ushort WORD_NONE = 0xffe;
    const ushort WORD_ANY = 0xfff;

    private readonly SCIPackage _package;
    private readonly Word[] _words;
    private readonly List<ParseTreeBranch> _branches;
    private readonly ParseRuleList _parserRules;
    private readonly Suffix[] _suffixes;
    private SaidData[] _saidTokens;
    private int _saidToken;

    private SaidData CurrToken => _saidToken < _saidTokens.Length ? _saidTokens[_saidToken] : TerminalToken;
    private static readonly SaidData TerminalToken = new(0xff);


    public Parser(SCIPackage package)
    {
        _package = package;

        var voc000 = (ResVocab000)package.GetResource<ResVocab>(0);
        IEnumerable<Word> words = voc000.GetWords();

        if (package.GetResource(ResType.Vocabulary, 1) is ResVocab001 vocTr)
            words = words.Union(vocTr.GetWords());

        _words = words.ToArray();

        var voc900 = (ResVocab900)package.GetResource<ResVocab>(900);
        _branches = voc900.GetBranches();
        _parserRules = BuildGNF();

        var voc901 = (ResVocab901)package.GetResource<ResVocab>(901);
        _suffixes = voc901.GetSuffixes();
    }

    public TokenizeResult Tokenize(string text)
    {
        TokenizeResult result = new();

        var reg = new Regex("([\\w\\d]+)");
        var matches = reg.Matches(text);
        foreach (var match in matches.Cast<Match>())
        {
            LookupResult lup = new() { Word = match.Value };
            List<ParsedWord> list = new();
            LookupWord(list, match.Value.ToLower());
            if (list.Count > 0)
                lup.Ids = list;
            result.Words.Add(lup);
        }

        return result;
    }

    private void LookupWord(List<ParsedWord> list, string word)
    {
        foreach (var w in _words.Where(w => w.Text == word))
            list.Add(new ParsedWord
            {
                Class = w.Class,
                Group = w.Group
            });

        foreach (var s in _suffixes)
            if (s.IsMatchReverse(word, out string newWord))
                LookupWord(list, newWord, s);
    }

    private void LookupWord(List<ParsedWord> list, string word, Suffix suffix)
    {
        foreach (var w in _words.Where(w => w.Text == word && (w.Class & suffix.InputClass) != 0))
            list.Add(new ParsedWord
            {
                Class = suffix.SuffixClass,
                Group = w.Group
            });
    }

    private ParseRuleList BuildGNF()
    {
        ParseRuleList ntlist = null;

        for (int i = 1; i < _branches.Count; i++)
        {
            var rule = VBuildRule(_branches[i]);
            ntlist = VocabAddRule(ntlist, rule);
        }

        ParseRuleList newTlist = VocabSplitRuleList(ntlist);
        ParseRuleList tlist = null;
        //Console.WriteLine($"Starting with {VocabRuleListLength(ntlist)} rules");

        int termrules;
        int iterations = 0;
        do
        {
            ParseRuleList newNewTlist = null;
            ParseRuleList ntseeker, tseeker;

            ntseeker = ntlist;
            while (ntseeker != null)
            {
                tseeker = newTlist;
                while (tseeker != null)
                {
                    ParseRule newrule = VInsert(ntseeker.Rule, tseeker.Rule);
                    if (newrule != null)
                        newNewTlist = VocabAddRule(newNewTlist, newrule);
                    tseeker = tseeker.Next;
                }

                ntseeker = ntseeker.Next;
            }

            tlist = VocabMergeRuleList(tlist, newTlist);
            newTlist = newNewTlist;
            termrules = VocabRuleListLength(newNewTlist);
            iterations++;
            //Console.WriteLine($"After iteration #{iterations}: {termrules} new term rules");
        } while (termrules > 0 && (iterations < 30));

        return tlist;
    }

    public bool Verbose { get; set; }

    private void Log(string value)
    {
        if (Verbose) Console.WriteLine(value);
    }

    public ParseTreeNode ParseGNF(IEnumerable<LookupResult> words)
    {
        ParseRuleList work = VocabCloneRuleListById(_parserRules, _branches[0].Data[1]);
        uint word = 0;
        int wordsCnt = words.Count();

        foreach (var w in words)
        {
            ParseRuleList newWork = null;
            ParseRuleList reducedRules = null;

            Log($"Adding word {word}...");

            var seeker = work;
            while (seeker != null)
            {
                if (seeker.Rule.NumSpecials <= (wordsCnt - word))
                    reducedRules = VocabAddRule(reducedRules, VSatisfyRule(seeker.Rule, w));

                seeker = seeker.Next;
            }

            if (reducedRules == null)
            {
                Log("No results.");
                return null;
            }

            if (word + 1 < wordsCnt)
            {
                seeker = reducedRules;
                while (seeker != null)
                {
                    if (seeker.Rule.NumSpecials != 0)
                    {
                        var myId = seeker.Rule.Data[seeker.Rule.FirstSpecial];

                        var subseeker = _parserRules;
                        while (subseeker != null)
                        {
                            if (subseeker.Rule.Id == myId)
                                newWork = VocabAddRule(newWork, VInsert(seeker.Rule, subseeker.Rule));

                            subseeker = subseeker.Next;
                        }
                    }

                    seeker = seeker.Next;
                }
            }
            else // last word
                newWork = reducedRules;

            work = newWork;
            Log($"Now at {VocabRuleListLength(work)} candidates");
            if (work == null)
            {
                Log("No results.");
                return null;
            }

            word++;
        }

        var root = new ParseTreeNode
        {
            Type = ParseTypes.Branch,
            Left = new ParseTreeNode
            {
                Type = ParseTypes.Leaf,
                Value = 0x141
            },
            Right = new ParseTreeNode
            {
                Type = ParseTypes.Branch
            }
        };

        var temp = root.Right.Append(_branches[0].Id);
        temp.WriteSubexpression(work.Rule, 0);

        return root;
    }

    private static int VocabRuleListLength(ParseRuleList list)
    {
        int i = 0;
        while (list != null)
        {
            list = list.Next;
            i++;
        }
        return i;
    }

    private static ParseRuleList VocabMergeRuleList(ParseRuleList l1, ParseRuleList l2)
    {
        var retval = l1;
        var seeker = l2;
        while (seeker != null)
        {
            retval = VocabAddRule(retval, seeker.Rule);
            seeker = seeker.Next;
        }
        return retval;
    }

    private static ParseRule VInsert(ParseRule turkey, ParseRule stuffing)
    {
        var firstnt = turkey.FirstSpecial;
        while (firstnt < turkey.Data.Count && (turkey.Data[firstnt] & TOKEN_NON_NT) != 0)
            firstnt++;

        if (firstnt == turkey.Data.Count || turkey.Data[firstnt] != stuffing.Id)
            return null;

        var rule = new ParseRule(turkey);
        rule.NumSpecials += stuffing.NumSpecials - 1;
        rule.FirstSpecial = firstnt + stuffing.FirstSpecial;

        rule.Data = rule.Data.GetRange(0, firstnt)
            .Concat(stuffing.Data)
            .ToList();

        if (firstnt < turkey.Data.Count - 1)
        {
            rule.Data = rule.Data.GetRange(0, firstnt + stuffing.Data.Count)
                .Concat(turkey.Data.GetRange(firstnt + 1, turkey.Data.Count - firstnt - 1))
                .ToList();
        }

        return rule;
    }

    private static ParseRuleList VocabSplitRuleList(ParseRuleList list)
    {
        if (list.Next == null || list.Next.Terminal != 0)
        {
            var tmp = list.Next;
            list.Next = null;
            return tmp;
        }
        return VocabSplitRuleList(list.Next);
    }

    private static ParseRuleList VocabAddRule(ParseRuleList list, ParseRule rule)
    {
        if (rule == null) return list;
        if (rule.Data.Count == 0) return list;

        var newElem = new ParseRuleList(rule);
        if (list != null)
        {
            var term = newElem.Terminal;
            var seeker = list;
            while (seeker.Next != null)
            {
                if (seeker.Next.Terminal == term)
                {
                    if (seeker.Next.Rule == rule)
                        return list;
                }
                seeker = seeker.Next;
            }

            newElem.Next = seeker.Next;
            seeker.Next = newElem;
            return list;
        }

        return newElem;
    }

    private static ParseRule VBuildRule(ParseTreeBranch branch)
    {
        int tokens = 0, tokenpos = 0;

        while (tokenpos < 10 && tokenpos < branch.Data.Count)
        {
            int type = branch.Data[tokenpos];
            tokenpos += 2;

            if ((type == VOCAB_TREE_NODE_COMPARE_TYPE) || (type == VOCAB_TREE_NODE_COMPARE_GROUP) || (type == VOCAB_TREE_NODE_FORCE_STORAGE))
                ++tokens;
            else if (type > VOCAB_TREE_NODE_LAST_WORD_STORAGE)
                tokens += 5;
            else
                return null; // invalid
        }

        var rule = new ParseRule
        {
            Id = branch.Id,
            NumSpecials = tokenpos >> 1,
            FirstSpecial = 0,
            Data = new List<uint>(tokens)
        };

        for (int i = 0; i < tokenpos; i += 2)
        {
            var type = branch.Data[i];
            var value = branch.Data[i + 1];

            if (type == VOCAB_TREE_NODE_COMPARE_TYPE)
                rule.Data.Add(value | TOKEN_TERMINAL_CLASS);
            else if (type == VOCAB_TREE_NODE_COMPARE_GROUP)
                rule.Data.Add(value | TOKEN_TERMINAL_GROUP);
            else if (type == VOCAB_TREE_NODE_FORCE_STORAGE)
                rule.Data.Add(value | TOKEN_STUFFING_WORD);
            else
            {
                rule.Data.Add(TOKEN_OPAREN);
                rule.Data.Add(type | TOKEN_STUFFING_LEAF);
                rule.Data.Add(value | TOKEN_STUFFING_LEAF);

                if (i == 0)
                    rule.FirstSpecial = rule.Data.Count;

                rule.Data.Add(value);
                rule.Data.Add(TOKEN_CPAREN);
            }
        }

        return rule;
    }


    private static ParseRule VSatisfyRule(ParseRule rule, LookupResult input)
    {
        if (rule.NumSpecials == 0) return null;

        var dep = rule.Data[rule.FirstSpecial];

        int count = 0;
        uint match = 0;
        List<uint> matches = new();

        foreach (var w in input.Ids)
        {
            if (((dep & TOKEN_TERMINAL_CLASS) != 0 && (dep & 0xffff & (uint)w.Class) != 0) ||
                ((dep & TOKEN_TERMINAL_GROUP) != 0 && (dep & 0xffff & w.Group) != 0))
            {
                if (count == 0)
                    match = TOKEN_STUFFING_WORD | w.Group;
                else
                    matches.Add(TOKEN_STUFFING_WORD | w.Group);
                count++;
            }
        }

        if (count > 0)
        {
            var retval = rule.Clone();
            retval.Data[rule.FirstSpecial] = match;
            if (count > 1)
                retval.Data.InsertRange(rule.FirstSpecial + 1, matches);
            retval.NumSpecials--;
            retval.FirstSpecial = 0;

            if (retval.NumSpecials > 0)
            {
                for (int i = rule.FirstSpecial; i < rule.Data.Count; i++)
                {
                    var tmp = retval.Data[i];
                    if (!((tmp & TOKEN_NON_NT) != 0 || (tmp & TOKEN_TERMINAL) != 0))
                    {
                        retval.FirstSpecial = i;
                        break;
                    }
                }
            }
            return retval;
        }

        return null;
    }

    private ParseRuleList VocabCloneRuleListById(ParseRuleList list, ushort id)
    {
        ParseRuleList result = null;
        var seeker = list;
        while (seeker != null)
        {
            if (seeker.Rule.Id == id)
                result = VocabAddRule(result, seeker.Rule.Clone());
            seeker = seeker.Next;
        }
        return result;
    }

    public ParseTreeNode BuildSaidTree(SaidData[] said)
    {
        _saidTokens = said;
        _saidToken = 0;

        ParseTreeNode saidTree = ParseTreeNode.CreateBranch(
            ParseTreeNode.CreateLeaf(0x141),
            ParseTreeNode.CreateBranch(
                    ParseTreeNode.CreateLeaf(0x13f),
                    null)
        );

        if (!ParseSpec(saidTree.Right))
            return null;

        if (_saidToken != _saidTokens.Length)
        {
            _saidTokens = null;
            return null;
        }

        _saidTokens = null;
        return saidTree;
    }

    private bool ParseWord(ParseTreeNode parentNode)
    {
        if (CurrToken.IsOperator)
            return false;
        parentNode.Right = ParseTreeNode.CreateWord(CurrToken.Data);
        _saidToken++;
        return true;
    }

    private bool ParsePart2(ParseTreeNode parentNode, out bool nonempty)
    {
        // Save
        int curToken = _saidToken;
        var oldRight = parentNode.Right;

        var newNode = ParseTreeNode.CreateBranch(null, null);
        nonempty = true;

        if (ParseSlash(newNode))
        {
            parentNode.AttachSubtree(0x142, 0x14a, newNode);
            return true;
        }

        if (CurrToken.Letter == "[")
        {
            _saidToken++;
            if (ParsePart2(newNode, out nonempty))
            {
                if (CurrToken.Letter == "]")
                {
                    _saidToken++;
                    parentNode.AttachSubtree(0x152, 0x142, newNode);
                    return true;
                }
            }
        }

        if (CurrToken.Letter == "/")
        {
            _saidToken++;
            nonempty = false;
            return true;
        }

        // Rollback
        _saidToken = curToken;
        parentNode.Right = oldRight;
        return false;
    }

    private bool ParsePart3(ParseTreeNode parentNode, out bool nonempty)
    {
        // Save
        int curToken = _saidToken;
        var oldRight = parentNode.Right;

        var newNode = ParseTreeNode.CreateBranch(null, null);
        nonempty = true;

        if (ParseSlash(newNode))
        {
            parentNode.AttachSubtree(0x143, 0x14a, newNode);
            return true;
        }

        if (CurrToken.Letter == "[")
        {
            _saidToken++;
            if (ParsePart3(newNode, out nonempty))
            {
                if (CurrToken.Letter == "]")
                {
                    _saidToken++;
                    parentNode.AttachSubtree(0x152, 0x143, newNode);
                    return true;
                }
            }
        }

        if (CurrToken.Letter == "/")
        {
            _saidToken++;
            nonempty = false;
            return true;
        }

        // Rollback
        _saidToken = curToken;
        parentNode.Right = oldRight;
        return false;
    }

    private bool ParseSlash(ParseTreeNode parentNode)
    {
        // Save
        int curToken = _saidToken;
        var oldRight = parentNode.Right;

        if (CurrToken.Letter == "/")
        {
            _saidToken++;
            if (ParseExpr(parentNode))
                return true;
        }

        // Rollback
        _saidToken = curToken;
        parentNode.Right = oldRight;
        return false;
    }

    private bool ParseRef(ParseTreeNode parentNode)
    {
        // Save
        int curToken = _saidToken;
        var oldRight = parentNode.Right;

        var newNode = ParseTreeNode.CreateBranch(null, null);
        var newParent = parentNode;

        if (CurrToken.Letter == "<")
        {
            _saidToken++;
            if (ParseList(newNode))
            {
                newParent.AttachSubtree(0x144, 0x14f, newNode);
                newParent = newParent.Right;
                newNode = ParseTreeNode.CreateBranch(null, null);

                if (ParseRef(newNode))
                    newParent.AttachSubtree(0x141, 0x144, newNode);

                return true;
            }
        }


        if (CurrToken.Letter == "[")
        {
            _saidToken++;
            if (ParseRef(newNode))
            {
                if (CurrToken.Letter == "]")
                {
                    _saidToken++;
                    parentNode.AttachSubtree(0x152, 0x144, newNode);
                    return true;
                }
            }
        }

        // Rollback
        _saidToken = curToken;
        parentNode.Right = oldRight;
        return false;
    }

    private bool ParseComma(ParseTreeNode parentNode)
    {
        // Save
        int curToken = _saidToken;
        var oldRight = parentNode.Right;

        if (CurrToken.Letter == ",")
        {
            _saidToken++;
            if (ParseList(parentNode))
                return true;
        }

        // Rollback
        _saidToken = curToken;
        parentNode.Right = oldRight;
        return false;
    }

    private bool ParseListEntry(ParseTreeNode parentNode)
    {
        // Save
        int curToken = _saidToken;
        var oldRight = parentNode.Right;

        var newNode = ParseTreeNode.CreateBranch(null, null);

        if (CurrToken.Letter == "[")
        {
            _saidToken++;
            if (ParseExpr(newNode))
            {
                if (CurrToken.Letter == "]")
                {
                    _saidToken++;
                    parentNode.AttachSubtree(0x152, 0x14c, newNode);
                    return true;
                }
            }
        }
        else if (CurrToken.Letter == "(")
        {
            _saidToken++;
            if (ParseExpr(newNode))
            {
                if (CurrToken.Letter == ")")
                {
                    _saidToken++;
                    parentNode.AttachSubtree(0x141, 0x14c, newNode);
                    return true;
                }
            }
        }
        else if (ParseWord(newNode))
        {
            parentNode.AttachSubtree(0x141, 0x153, newNode);
            return true;
        }

        // Rollback
        _saidToken = curToken;
        parentNode.Right = oldRight;
        return false;
    }

    private bool ParseList(ParseTreeNode parentNode)
    {
        // Save
        int curToken = _saidToken;
        var oldRight = parentNode.Right;

        var newParent = parentNode;
        if (ParseListEntry(newParent))
        {
            newParent = newParent.Right;
            ParseComma(newParent);
            return true;
        }

        // Rollback
        _saidToken = curToken;
        parentNode.Right = oldRight;
        return false;
    }

    private bool ParseExpr(ParseTreeNode parentNode)
    {
        // Save
        int curToken = _saidToken;
        var oldRight = parentNode.Right;

        var newNode = ParseTreeNode.CreateBranch(null, null);

        bool ret = false;
        var newParent = parentNode;
        if (ParseList(newNode))
        {
            ret = true;
            newParent.AttachSubtree(0x141, 0x14f, newNode);
            newParent = newParent.Right;
        }

        var found = ParseRef(newParent);

        if (found || ret) return true;

        // Rollback
        _saidToken = curToken;
        parentNode.Right = oldRight;
        return false;
    }

    private bool ParseSpec(ParseTreeNode parentNode)
    {
        var newNode = ParseTreeNode.CreateBranch(null, null);

        bool ret = false;

        ParseTreeNode newParent = parentNode;

        if (ParseExpr(newNode))
        {
            newParent.AttachSubtree(0x141, 0x149, newNode);
            newParent = newParent.Right;
            ret = true;
        }

        if (ParsePart2(newParent, out bool nonempty))
        {
            ret = true;

            if (nonempty)
                newParent = newParent.Right;

            if (ParsePart3(newParent, out nonempty))
            {
                if (nonempty)
                    newParent = newParent.Right;
            }
        }

        if (CurrToken.Letter == ">")
        {
            _saidToken++;
            newNode = ParseTreeNode.CreateBranch(null, ParseTreeNode.CreateLeaf(0xf9));
            newParent.AttachSubtree(0x14b, 0xf9, newNode);
        }

        return ret;
    }

    private int _outputDepth = 0;
    private void LogSpace()
    {
        for (int i = 0; i < _outputDepth; i++)
            Console.Write(' ');
    }

    private void Log(ParseTreeNode node)
    {
        if (node.Left.Type == ParseTypes.Branch)
        {
            Console.Write("< ");
            Log(node.Left);
            Console.Write(", ...>");
        }
        else
        {
            if (node.IsTerminal)
                Console.Write($"({node.Major:x3} {node.Minor:x3} {node.TerminalValue:x3})");
            else
                Console.Write($"({node.Major:x3} {node.Minor:x3} <...>)");
        }
    }

    public bool Match(ParseTreeNode parseTree, ParseTreeNode saidTree)
    {
        return MatchTrees(parseTree, saidTree) == 1;
    }

    private int MatchTrees(ParseTreeNode parseTree, ParseTreeNode saidTree)
    {
        if (Verbose)
        {
            _outputDepth++;
            LogSpace();
            Console.Write("matchTrees on ");
            Log(parseTree);
            Console.Write(" and ");
            Log(saidTree);
            Console.WriteLine();
        }

        bool inParen = saidTree.Minor == 0x14f || saidTree.Minor == 0x150;
        bool inBracket = saidTree.Major == 0x152;

        int ret;

        if (parseTree.Major != 0x141 &&
            saidTree.Major != 0x141 && saidTree.Major != 0x152 &&
            saidTree.Major != parseTree.Major)
        {
            ret = -1;
        }
        else if (saidTree.IsTerminal && parseTree.IsTerminal)
        {
            var saidVal = saidTree.TerminalValue;

            if (saidVal == WORD_NONE)
                ret = -1;
            else if (saidVal == WORD_ANY)
                ret = 1;
            else
            {
                ret = -1;

                parseTree = parseTree.Right.Right;
                do
                {
                    int parseVal = parseTree.Value;
                    if (parseVal == WORD_ANY || parseVal == saidVal)
                    {
                        ret = 1;
                        break;
                    }
                    parseTree = parseTree.Right;
                }
                while (parseTree != null);
            }
        }
        else if (saidTree.IsTerminal && !parseTree.IsTerminal)
        {
            if (parseTree.Major == 0x141 || parseTree.Major == saidTree.Major)
                ret = ScanParseChildren(parseTree.Right.Right, saidTree);
            else
                ret = 0;
        }
        else if (parseTree.IsTerminal)
        {
            if (saidTree.Major == 0x141 || saidTree.Major == 0x152 ||
                saidTree.Major == parseTree.Major)
                ret = ScanSaidChildren(parseTree, saidTree.Right.Right,
                    inParen ? ScanSaidType.OR : ScanSaidType.AND);
            else
                ret = 0;
        }
        else if (saidTree.Major != 0x141 && saidTree.Major != 0x152 && saidTree.Major != parseTree.Major)
        {
            ret = ScanParseChildren(parseTree.Right.Right, saidTree);
        }
        else
        {
            ret = ScanSaidChildren(parseTree.Right.Right, saidTree.Right.Right,
                inParen ? ScanSaidType.OR : ScanSaidType.AND);
        }

        if (inBracket && ret == 0)
            ret = 1;

        if (Verbose)
        {
            LogSpace();
            Console.WriteLine($"matchTrees returning {ret}");
            _outputDepth--;
        }

        return ret;
    }

    private int ScanSaidChildren(ParseTreeNode parseTree, ParseTreeNode saidTree, ScanSaidType type)
    {
        if (Verbose)
        {
            _outputDepth++;
            LogSpace();
            Console.Write($"scanSaid({type}) on ");
            Log(parseTree);
            Console.Write(" and ");
            Log(saidTree);
            Console.WriteLine();
        }

        int ret = 1;
        while (saidTree != null)
        {
            var saidChild = saidTree.Left;
            if (saidChild.Major != 0x145)
            {
                ret = ScanParseChildren(parseTree, saidChild);
                if (type == ScanSaidType.AND && ret != 1)
                    break;
                if (type == ScanSaidType.OR && ret == 1)
                    break;
            }

            saidTree = saidTree.Right;
        }

        if (Verbose)
        {
            LogSpace();
            Console.WriteLine($"scanSaid returning {ret}");
            _outputDepth--;
        }

        return ret;
    }

    enum ScanSaidType
    {
        OR, AND
    }

    private int ScanParseChildren(ParseTreeNode parseTree, ParseTreeNode saidTree)
    {
        if (Verbose)
        {
            _outputDepth++;
            LogSpace();
            Console.Write("scanParse on ");
            Log(parseTree);
            Console.Write(" and ");
            Log(saidTree);
            Console.WriteLine();
        }

        if (saidTree.Major == 0x14b)
        {
            if (Verbose)
            {
                LogSpace();
                Console.WriteLine("scanParse returning 1 (0x14B)");
                _outputDepth--;
            }
            return 1;
        }

        bool inParen = saidTree.Minor == 0x14f || saidTree.Minor == 0x150;
        bool inBracket = saidTree.Major == 0x152;

        int ret;

        if ((saidTree.Major == 0x141 || saidTree.Major == 0x152) &&
            !saidTree.IsTerminal)
        {
            ret = ScanSaidChildren(parseTree, saidTree.Right.Right,
                inParen ? ScanSaidType.OR : ScanSaidType.AND);
        }
        else if (parseTree != null && parseTree.Left.Type == ParseTypes.Branch)
        {
            ret = 0;
            int subresult = 0;

            while (parseTree != null)
            {
                var parseChild = parseTree.Left;

                if (Verbose)
                {
                    LogSpace();
                    Console.Write("scanning next: ");
                    Log(parseChild);
                    Console.WriteLine();
                }

                if (parseChild.Major == saidTree.Major ||
                    parseChild.Major == 0x141)
                    subresult = MatchTrees(parseChild, saidTree);

                if (subresult != 0)
                    ret = subresult;

                if (ret == 1)
                    break;

                parseTree = parseTree.Right;
            }
        }
        else
        {
            ret = MatchTrees(parseTree, saidTree);
        }

        if (inBracket && ret == 0)
            ret = 1;

        if (Verbose)
        {
            LogSpace();
            Console.WriteLine($"scanParse returning {ret}");
            _outputDepth--;
        }

        return ret;
    }
}
