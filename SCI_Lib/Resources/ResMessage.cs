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
        private List<MessageRecord> _records;

        public List<MessageRecord> GetMessages()
        {
            if (_records != null) return _records;

            var data = GetContent();
            List<MessageRecord> records;

            using (var stream = new MemoryStream(data))
            {
                header = stream.ReadUIntBE();
                var ver = header / 1000;
                records = ver switch
                {
                    2 => ReadV2(stream),
                    3 => ReadV3(stream),
                    4 => ReadV4(stream),
                    _ => throw new NotImplementedException(),
                };
            }

            foreach (var r in records)
                r.ReadText(data, GameEncoding);

            return _records = records;
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
        }

        public override byte[] GetPatch()
        {
            var messages = GetMessages();

            ByteBuilder bb = new ByteBuilder();

            if (messages[0] is MessageRecordV2)
                SaveV2(messages, bb);
            else if (messages[0] is MessageRecordV3)
                SaveV3(messages, bb);
            else if (messages[0] is MessageRecordV4)
                SaveV4(messages, bb);
            else
                throw new NotImplementedException();

            return bb.GetArray();
        }

        private List<MessageRecord> ReadV2(MemoryStream stream)
        {
            ushort count = stream.ReadUShortBE();

            List<MessageRecord> records = new List<MessageRecord>();
            for (int i = 0; i < count; i++)
                records.Add(new MessageRecordV2(stream));
            return records;
        }

        private void SaveV2(List<MessageRecord> messages, ByteBuilder bb)
        {
            bb.AddIntBE(header);
            bb.AddUShortBE((ushort)messages.Count);

            foreach (var mes in messages)
                mes.WriteHeader(bb);

            foreach (var mes in messages)
                mes.WriteText(bb, GameEncoding);
        }

        private static List<MessageRecord> ReadV3(Stream stream)
        {
            ushort end = stream.ReadUShortBE();
            ushort count = stream.ReadUShortBE();

            List<MessageRecord> records = new List<MessageRecord>(count);
            for (int i = 0; i < count; i++)
                records.Add(new MessageRecordV3(stream));
            return records;
        }

        private void SaveV3(List<MessageRecord> messages, ByteBuilder bb)
        {
            bb.AddIntBE(header);
            int endOffset = bb.Position;
            bb.AddShortBE(0);
            bb.AddUShortBE((ushort)messages.Count);

            foreach (var mes in messages)
                mes.WriteHeader(bb);

            foreach (var mes in messages)
                mes.WriteText(bb, GameEncoding);

            bb.SetUShortBE(endOffset, (ushort)(bb.Position - endOffset));
        }

        private static List<MessageRecord> ReadV4(MemoryStream stream)
        {
            ushort end = stream.ReadUShortBE();
            ushort unknown = stream.ReadUShortBE();
            ushort count = stream.ReadUShortBE();

            List<MessageRecord> records = new List<MessageRecord>(count);
            for (int i = 0; i < count; i++)
                records.Add(new MessageRecordV4(stream));
            return records;
        }

        private void SaveV4(List<MessageRecord> messages, ByteBuilder bb)
        {
            bb.AddIntBE(header);
            int endOffset = bb.Position;
            bb.AddShortBE(0);
            bb.AddUShortBE((ushort)messages.Count);
            bb.AddUShortBE((ushort)messages.Count);

            foreach (var mes in messages)
                mes.WriteHeader(bb);

            foreach (var mes in messages)
                mes.WriteText(bb, GameEncoding);

            bb.SetUShortBE(endOffset, (ushort)(bb.Position - endOffset));
        }
    }
}
