using System;

namespace SCI_Lib.Resources.Vocab
{
    public class Word
    {
        public Word(string text, int id)
        {
            Text = text;
            Class = (WordClass)(ushort)(id >> 12);
            Group = (ushort)(id & 0xfff);
        }

        public Word(string text, ushort cl, ushort group)
        {
            Text = text;
            Class = (WordClass)cl;
            Group = group;
        }

        public int Id => ((ushort)Class << 12) | Group;

        public string Text { get; set; }
        public WordClass Class { get; set; }
        public ushort Group { get; set; }

        public override string ToString() => Text;
    }

    [Flags]
    public enum WordClass : ushort
    {
        Unknown = 0x000,
        Number = 0x001,
        Punctuation = 0x002,
        Conjunction = 0x004,
        Association = 0x008,
        Proposition = 0x010,
        Article = 0x020,
        QualifyingAdjective = 0x040,
        RelativePronoun = 0x080,
        Noun = 0x100,
        IndicativeVerb = 0x200,
        Adverb = 0x400,
        ImperativeVerb = 0x800
    }
}
