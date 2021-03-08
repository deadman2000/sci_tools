using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCI_Lib.Resources
{
    public class ResMessage : Resource
    {
        private uint header;

        public List<MessageRecord> GetMessages()
        {
            var data = GetContent();
            List<MessageRecord> records;

            using (var stream = new MemoryStream(data))
            {
                header = stream.ReadUIntBE();
                var ver = header / 1000;
                records = ver switch
                {
                    3 => ReadV3(stream),
                    4 => ReadV4(stream),
                    _ => throw new NotImplementedException(),
                };
            }

            foreach (var r in records)
                r.ReadText(data, GameEncoding);

            return records;
        }

        public override string[] GetStrings()
        {
            return GetMessages().Select(m => m.Text).ToArray();
        }

        public override void SetStrings(string[] strings)
        {
            var messages = GetMessages();
            if (strings.Length != messages.Count)
                throw new Exception("Line count mismatch");

            for (int i = 0; i < strings.Length; i++)
            {
                if (strings[i] != null)
                    messages[i].Text = strings[i];
            }

            SetMessages(messages);
        }

        public void SetMessages(List<MessageRecord> messages)
        {
            ByteBuilder bb = new ByteBuilder();

            if (messages[0] is MessageRecordV3)
                SaveV3(messages, bb);
            else if (messages[0] is MessageRecordV4)
                SaveV4(messages, bb);
            else
                throw new NotImplementedException();

            SavePatch(bb.GetArray());
        }

        private List<MessageRecord> ReadV3(Stream stream)
        {
            ushort end = stream.ReadUShortBE();
            ushort count = stream.ReadUShortBE();

            List<MessageRecord> records = new List<MessageRecord>();
            for (int i = 0; i < count; i++)
                records.Add(new MessageRecordV3(stream));
            return records;
        }

        private void SaveV3(List<MessageRecord> messages, ByteBuilder bb)
        {
            bb.AddIntBE(header);
            int endOffset = bb.Position;
            bb.AddShortBE(0);
            bb.AddShortBE((ushort)messages.Count);

            object[] extra = new object[messages.Count];

            for (int i = 0; i < messages.Count; i++)
            {
                extra[i] = messages[i].WriteHeader(bb);
            }

            for (int i = 0; i < messages.Count; i++)
            {
                messages[i].WriteText(bb, extra[i], GameEncoding);
            }

            bb.SetShortBE(endOffset, (ushort)(bb.Position - endOffset));
        }

        private List<MessageRecord> ReadV4(MemoryStream stream)
        {
            ushort end = stream.ReadUShortBE();
            ushort unknown = stream.ReadUShortBE();
            ushort count = stream.ReadUShortBE();

            List<MessageRecord> records = new List<MessageRecord>();
            for (int i = 0; i < count; i++)
                records.Add(new MessageRecordV4(stream));
            return records;
        }

        private void SaveV4(List<MessageRecord> messages, ByteBuilder bb)
        {
            bb.AddIntBE(header);
            int endOffset = bb.Position;
            bb.AddShortBE(0);
            bb.AddShortBE((ushort)messages.Count);
            bb.AddShortBE((ushort)messages.Count);

            object[] extra = new object[messages.Count];

            for (int i = 0; i < messages.Count; i++)
            {
                extra[i] = messages[i].WriteHeader(bb);
            }

            for (int i = 0; i < messages.Count; i++)
            {
                messages[i].WriteText(bb, extra[i], GameEncoding);
            }

            bb.SetShortBE(endOffset, (ushort)(bb.Position - endOffset));
        }
    }
}
