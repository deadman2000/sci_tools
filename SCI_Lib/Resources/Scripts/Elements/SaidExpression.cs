using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public class SaidExpression : BaseElement
    {
        private string _label;
        public SaidData[] Expression { get; private set; }

        public SaidExpression(Script script, ushort address, SaidData[] saidDatas)
            : base(script, address)
        {
            Expression = saidDatas;
        }

        protected override void WriteData(ByteBuilder bb)
        {
            foreach (var data in Expression)
            {
                if (data.IsOperator)
                    bb.AddByte((byte)data.Data);
                else
                    bb.AddUShortLE(data.Data);
            }
            bb.AddByte(0xff);
        }

        public override void WriteOffset(ByteBuilder bb)
        {
        }

        public override string ToString() => Label;

        public bool Set(string expression)
        {
            var parsed = Parse(expression);
            if (IsEqual(Expression, parsed)) return false;
            Expression = parsed;
            _label = null;
            return true;
        }

        public bool Set(IEnumerable<SaidData> saids)
        {
            var arr = saids.ToArray();
            if (IsEqual(Expression, arr)) return false;
            Expression = arr;
            _label = null;
            return true;
        }

        private static bool IsEqual(SaidData[] e1, SaidData[] e2)
        {
            //return Array.Equals(e1, e2);
            if (e1.Length != e2.Length) return false;
            for (int i = 0; i < e1.Length; i++)
                if (!e1[i].Equals(e2[i])) return false;
            return true;
        }

        private SaidData[] Parse(string expression)
        {
            var data = new List<SaidData>();
            var buff = new List<char>();
            for (int i = 0; i < expression.Length; i++)
            {
                var c = expression[i];
                if (char.IsWhiteSpace(c)) continue;
                if (char.IsLetterOrDigit(c) || c == '*' || c == '!')
                    buff.Add(c);
                else
                {
                    if (buff.Count > 0)
                    {
                        ushort id = GetWord(buff);
                        data.Add(new SaidData(id));
                    }

                    data.Add(new SaidData(c));
                }
            }

            if (buff.Count > 0)
            {
                ushort id = GetWord(buff);
                data.Add(new SaidData(id));
            }

            return data.ToArray();
        }

        private ushort GetWord(List<char> buff)
        {
            var word = new string(buff.ToArray());
            buff.Clear();
            var ids = Script.Package.GetWordId(word) ?? throw new Exception($"Word not found {word}");
            if (ids.Length > 1) Console.WriteLine($"WARN: Multiple ids for word '{word}'");
            return ids[0];
        }

        public bool Normalize()
        {
            bool changed = false;
            HashSet<ushort> words = new();
            List<SaidData> list = new();
            foreach (var e in Expression)
            {
                if (e.IsOperator)
                {
                    list.Add(e);
                    if (e.Letter != ",")
                    {
                        words.Clear();
                    }
                }
                else // is word
                {
                    if (words.Contains(e.Data)) // Dubl found
                    {
                        changed = true;
                        if (list.Any() && list[^1].Letter == ",") // Remove comma
                            list.RemoveAt(list.Count - 1);
                    }
                    else
                    {
                        list.Add(e);
                        words.Add(e.Data);
                    }
                }
            }
            if (changed)
                Set(list);
            return changed;
        }

        public override string Label => _label ??= string.Join("", Expression.Select(s => s.ToString(Script.Package.GetWords())).ToArray());

        public string Hex => string.Join(" ", Expression.Select(s => s.Hex).ToArray());
    }
}
