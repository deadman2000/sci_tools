using SCI_Lib.Utils;
using System;
using System.Collections.Generic;

namespace SCI_Lib.Resources.Scripts.Sections
{
    public class SynonymSecion : Section
    {
        public List<Synonym> Synonyms { get; set; }

        public override void Read(byte[] data, ushort offset, int length)
        {
            Synonyms = new List<Synonym>();
            var end = offset + length;
            while (true)
            {
                var wordA = ReadShortBE(data, ref offset);
                if (wordA == 0xffff) break;

                var wordB = ReadShortBE(data, ref offset);
                if (offset >= end) throw new FormatException();
                Synonyms.Add(new Synonym
                {
                    WordA = wordA,
                    WordB = wordB
                });
            }
        }

        public override void Write(ByteBuilder bb)
        {
            foreach (var syn in Synonyms)
            {
                bb.AddShortBE(syn.WordA);
                bb.AddShortBE(syn.WordB);
            }
            bb.AddShortBE(0xffff);
        }
    }

    public struct Synonym
    {
        public ushort WordA;
        public ushort WordB;
    }
}