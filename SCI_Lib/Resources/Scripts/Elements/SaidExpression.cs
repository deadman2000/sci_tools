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

        public void Set(string expression)
        {
            Expression = Parse(expression);
            _label = null;
        }

        private SaidData[] Parse(string expression)
        {
            var data = new List<SaidData>();
            var buff = new List<char>();
            for (int i = 0; i < expression.Length; i++)
            {
                var c = expression[i];
                if (char.IsWhiteSpace(c)) continue;
                if (char.IsLetterOrDigit(c))
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
            var ids = Script.Package.GetWordId(word);
            if (ids == null) throw new Exception($"Word not found {word}");
            if (ids.Length > 1) throw new Exception($"Multiple ids for word '{word}'");
            return ids[0];
        }

        public override string Label => _label ??= string.Join("", Expression.Select(s => s.ToString(Script.Package.GetWords())).ToArray());

        public string Hex => string.Join(" ", Expression.Select(s => s.Hex).ToArray());
    }
}
