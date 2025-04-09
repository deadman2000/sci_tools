using SCI_Lib.Utils;
using System;
using System.Collections.Generic;

namespace SCI_Lib.Resources.Scripts.Sections
{
    public class SynonymSecion : Section
    {
        public List<Synonym> Synonyms { get; set; } = new List<Synonym>();

        public override void Read(byte[] data, ushort offset, int length)
        {
            Synonyms.Clear();
            var end = offset + length;
            while (true)
            {
                var wordA = ReadUShortBE(data, ref offset);
                if (wordA == 0xffff) break;

                var wordB = ReadUShortBE(data, ref offset);
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
                bb.AddUShortBE(syn.WordA);
                bb.AddUShortBE(syn.WordB);
            }
            bb.AddUShortBE(0xffff);
        }
    }

    public struct Synonym
    {
        public ushort WordA;
        public ushort WordB;
    }
}