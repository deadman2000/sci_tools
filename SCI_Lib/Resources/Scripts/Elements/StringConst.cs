﻿using SCI_Lib.Utils;
using System;

namespace SCI_Lib.Resources.Scripts.Elements
{
    public class StringConst : BaseElement
    {
        public StringConst(BaseScript script, byte[] data, ushort offset, int length)
            : base(script, offset)
        {
            Bytes = Helpers.GetBytes(data, offset, length);
        }

        public StringConst(BaseScript script, string str, ushort offset)
            : base(script, offset)
        {
            Bytes = GameEncoding.GetBytes(str);
        }

        public byte[] Bytes { get; set; }

        public string Value
        {
            get => GameEncoding.GetString(Bytes);
            set => Bytes = GameEncoding.GetBytes(value);
        }

        public char[] Chars => GameEncoding.GetChars(Bytes);

        public string ValueSlashEsc => BaseEscaper.Slash.Escape(Chars);

        public bool IsClassName { get; set; } = false;

        public void SetValueUnescape(string val) => Bytes = GameEncoding.Unescape(GameEncoding.GetBytes(val));

        public string GetStringEscape() => GameEncoding.GetStringEscape(Bytes);

        public override string ToString() => $"string_{Address:x4} = \"{Value}\"";

        public override string Label => ToString();

        protected override void WriteData(ByteBuilder bb)
        {
            bb.AddBytes(Bytes);
            bb.AddByte(0);
        }
    }

    public class StringPart : BaseElement
    {
        private int _offset;

        public StringPart(StringConst str, int offset)
            : base(str.Owner, (ushort)(str.Address + offset))
        {
            if (offset >= str.Bytes.Length)
                throw new ArgumentException();

            _offset = offset;
            OrigString = str;
        }

        /*public override ushort Address
        {
            get
            {
                if (OrigString == null) return base.Address;

                return (ushort)(OrigString.Address + _offset);
            }
            set => base.Address = value;
        }*/

        public StringConst OrigString { get; private set; }

        public string String => OrigString.Value.Substring(_offset);

        protected override void WriteData(ByteBuilder bb)
        {
        }
    }
}
