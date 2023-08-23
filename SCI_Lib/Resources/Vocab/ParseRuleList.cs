using static SCI_Lib.Resources.Vocab.Parser;

namespace SCI_Lib.Resources.Vocab
{
    internal class ParseRuleList
    {
        public ParseRuleList(ParseRule rule)
        {
            Rule = rule;
            var term = rule.Data[rule.FirstSpecial];
            Terminal = (term & TOKEN_TERMINAL) != 0 ? term : 0;
        }

        public ParseRule Rule { get; set; }
        public ParseRuleList Next { get; internal set; }
        public uint Terminal { get; set; }
    }
}