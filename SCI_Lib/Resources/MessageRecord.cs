using SCI_Lib.Utils;
using System.IO;

namespace SCI_Lib.Resources
{
    public abstract class MessageRecord
    {
        public int TextOffset { get; set; }

        public string Text { get; set; }

        public byte Noun { get; set; }

        public byte Verb { get; set; }

        public byte Cond { get; set; }

        public byte Seq { get; set; }

        public byte Talker { get; set; }

        public void ReadText(byte[] data, GameEncoding encoding)
        {
            for (int i = TextOffset; i < data.Length; i++)
            {
                if (data[i] == 0)
                {
                    Text = encoding.GetString(data, TextOffset, i - TextOffset);
                    return;
                }
            }
        }

        public abstract object WriteHeader(ByteBuilder bb);

        public abstract void WriteText(ByteBuilder bb, object data, GameEncoding encoding);
    }

    class MessageRecordV3 : MessageRecord
    {
        public int Unknown { get; set; }

        public override string ToString() => $"noun: {Noun} verb:{Verb} cond:{Cond} seq:{Seq} talker:{Talker} unknown:{Unknown} text: {Text}";

        public MessageRecordV3(Stream stream)
        {
            Noun = stream.ReadB();
            Verb = stream.ReadB();
            Cond = stream.ReadB();
            Seq = stream.ReadB();
            Talker = stream.ReadB();
            TextOffset = stream.ReadUShortBE();
            Unknown = stream.Read3ByteBE();
        }

        public override object WriteHeader(ByteBuilder bb)
        {
            bb.AddByte(Noun);
            bb.AddByte(Verb);
            bb.AddByte(Cond);
            bb.AddByte(Seq);
            bb.AddByte(Talker);
            var offsetPos = bb.Position;
            bb.AddShortBE(0); // Text offset
            bb.AddThreeBytesBE(Unknown);
            return offsetPos;
        }

        public override void WriteText(ByteBuilder bb, object data, GameEncoding encoding)
        {
            ushort textOffset = (ushort)bb.Position;

            var bytes = encoding.GetBytes(Text);
            bb.AddBytes(bytes);
            bb.AddByte(0);

            bb.SetShortBE((int)data, textOffset);
        }
    }

    class MessageRecordV4 : MessageRecord
    {
        public byte NounOfRef { get; set; }

        public byte VerbOfRef { get; set; }

        public byte CondOfRef { get; set; }

        public byte Unknown { get; set; }

        public override string ToString() => $"noun: {Noun} verb:{Verb} cond:{Cond} seq:{Seq} talker:{Talker} text: {Text}";

        public MessageRecordV4(Stream stream)
        {
            Noun = stream.ReadB();
            Verb = stream.ReadB();
            Cond = stream.ReadB();
            Seq = stream.ReadB();
            Talker = stream.ReadB();
            TextOffset = stream.ReadUShortBE();
            NounOfRef = stream.ReadB();
            VerbOfRef = stream.ReadB();
            CondOfRef = stream.ReadB();
            Unknown = stream.ReadB();
        }

        public override object WriteHeader(ByteBuilder bb)
        {
            bb.AddByte(Noun);
            bb.AddByte(Verb);
            bb.AddByte(Cond);
            bb.AddByte(Seq);
            bb.AddByte(Talker);
            var offsetPos = bb.Position;
            bb.AddShortBE(0); // Text offset
            bb.AddByte(NounOfRef);
            bb.AddByte(VerbOfRef);
            bb.AddByte(CondOfRef);
            bb.AddByte(Unknown);
            return offsetPos;
        }

        public override void WriteText(ByteBuilder bb, object data, GameEncoding encoding)
        {
            ushort textOffset = (ushort)bb.Position;

            var bytes = encoding.GetBytes(Text);
            bb.AddBytes(bytes);
            bb.AddByte(0);

            bb.SetShortBE((int)data, textOffset);
        }
    }
}
