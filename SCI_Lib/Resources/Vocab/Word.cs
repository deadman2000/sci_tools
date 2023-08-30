using System;
using System.Linq;

namespace SCI_Lib.Resources.Vocab
{
    public class Word
    {
        public Word(string text, int id)
        {
            Text = text;
            Id = id;
            Class = GetClass(id);
            Group = GetGroup(id);
        }
        public Word(string text, ushort cl, ushort group)
            : this(text, (WordClass)cl, group)
        {
        }

        public Word(string text, WordClass cl, ushort group)
        {
            Text = text;
            Id = GetId(group, (ushort)cl);
            Class = cl;
            Group = group;
        }

        public static int GetId(ushort gr, ushort cl) => gr | (cl << 12);
        public static ushort GetGroup(int id) => (ushort)(id & 0xfff);
        public static WordClass GetClass(int id) => (WordClass)(ushort)(id >> 12);

        public int Id { get; }
        public WordClass Class { get; }
        public ushort Group { get; }

        public string Text { get; set; }

        public override string ToString() => Text;

        public bool IsEn => Text.All(c => c >= 'a' && c <= 'z');
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
