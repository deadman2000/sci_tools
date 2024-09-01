using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCI_Lib.Resources.Scripts1
{
    public class Heap : BaseScript
    {
        private ushort[] _stringOffsets;

        public List<Object1> Objects { get; } = new List<Object1>();

        public LocalVar[] LocalVars { get; private set; }

        public List<StringConst> Strings { get; set; } = new List<StringConst>();

        public Heap(Resource res) : base(res)
        {
            Read();
        }

        public override IScriptInstance GetInstance(string name) => Objects.FirstOrDefault(o => o.Name == name);

        public override IScriptInstance GetInstance(string name, string superName) => Objects.FirstOrDefault(o => o.Name == name && o.Super.Name == superName);

        public override IEnumerable<StringConst> AllStrings() => Strings;

        private void Read()
        {
            var data = Resource.GetContent();
            using var heap = new MemoryStream(data);

            var stringOffset = heap.ReadUShortBE();
            _stringOffsets = ReadOffsets(heap, stringOffset);

            // Skip all to strings
            {
                var i = heap.ReadUShortBE();
                if (i > 0) heap.Seek(i * 2, SeekOrigin.Current);

                while (true)
                {
                    var magic = heap.ReadUShortBE();
                    if (magic == 0) break;
                    if (magic != 0x1234) throw new Exception("Wrong script format");
                    i = heap.ReadUShortBE();
                    heap.Seek((i - 2) * 2, SeekOrigin.Current);
                }
            }

            while (heap.Position < stringOffset)
            {
                var offset = (ushort)heap.Position;
                var str = heap.ReadString(Package.GameEncoding);
                var strConst = new StringConst(this, str, offset);
                Strings.Add(strConst);
            }

            heap.Seek(2, SeekOrigin.Begin);
            var localsCount = heap.ReadUShortBE();
            LocalVars = new LocalVar[localsCount];
            for (int i = 0; i < localsCount; i++)
            {
                ushort pos = (ushort)heap.Position;
                ushort val = heap.ReadUShortBE();

                var str = GetStringByOffset(pos, val);

                LocalVars[i] = new LocalVar { Value = val, StringValue = str };
            }

            while (true)
            {
                var offset = (ushort)heap.Position;
                var magic = heap.ReadUShortBE();
                if (magic == 0)
                    break;

                if (magic != 0x1234) throw new Exception("Wrong script format");

                var obj = new Object1(this, offset);
                obj.ReadHeap(heap);
                Objects.Add(obj);
            }
        }

        private static ushort[] ReadOffsets(MemoryStream stream, ushort offset)
        {
            var oldPos = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);

            var count = stream.ReadUShortBE();
            var offsets = new ushort[count];
            for (int i = 0; i < count; i++)
                offsets[i] = stream.ReadUShortBE();

            stream.Seek(oldPos, SeekOrigin.Begin);
            return offsets;
        }

        public override byte[] GetBytes()
        {
            ByteBuilder heap = new();
            heap.AddUShortBE(0); // string pointers offset
            heap.AddUShortBE((ushort)LocalVars.Length);
            foreach (var v in LocalVars)
                heap.AddUShortBE(v.Value);

            foreach (var obj in Objects)
                obj.Write(heap);

            heap.AddUShortBE(0); // End of objects

            // Write strings
            var strs = Strings.OrderBy(c => c.Address).ToArray();
            foreach (var s in strs)
                s.Write(heap);

            // Write strings refs
            heap.SetUShortBE(0, (ushort)heap.Position);
            heap.AddUShortBE((ushort)_stringOffsets.Length);
            foreach (var addr in _stringOffsets)
                heap.AddUShortBE(addr); // TODO replace by ref

            return heap.GetArray();
        }

        public StringConst GetStringByOffset(ushort pos, ushort offset)
        {
            if (!_stringOffsets.Contains(pos)) return null;

            var el = GetElement(offset);
            if (el != null)
            {
                if (el is not StringConst sc) throw new Exception();
                return sc;
            }
            return null;
        }
    }
}
