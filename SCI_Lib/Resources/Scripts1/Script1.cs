using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCI_Lib.Resources.Scripts1
{
    public class Script1 : BaseScript, ICodeBlock
    {
        private Heap _heap;
        private int _unknown;

        public List<Code> Operators { get; private set; }

        public GlobalRef[] Exports { get; private set; }

        public BaseElement[] HeapPointers { get; private set; }

        public List<Object1> Objects => _heap.Objects;

        public Script1(Resource res) : base(res)
        {
            Read();
        }

        public override string ToString() => $"Script {Resource}";

        public override IScriptInstance GetInstance(string name) => Objects.FirstOrDefault(o => o.Name == name);

        public override IScriptInstance GetInstance(string name, string superName) => Objects.FirstOrDefault(o => o.Name == name && o.Super.Name == superName);

        public override IEnumerable<StringConst> AllStrings() => Array.Empty<StringConst>();

        public List<Method> Procedures { get; } = new();

        public BaseScript CodeOwner => this;

        public override BaseElement GetElement(ushort offset)
        {
            var element = base.GetElement(offset);
            if (element != null)
                return element;

            return _heap.GetElement(offset);
        }

        private void Read()
        {
            // https://github.com/scummvm/scummvm/blob/master/engines/sci/engine/script.cpp#L390
            // https://github.com/icefallgames/SCICompanion/blob/master/SCICompanionLib/Src/Compile/CompiledScript.cpp#L226
            var data = Resource.GetContent();
            using var stream = new MemoryStream(data);

            var heapRes = Package.GetResource<ResHeap>(Resource.Number);
            _heap = heapRes.GetHeap();

            var heapPointersOffset = stream.ReadUShortBE();
            _unknown = stream.ReadIntBE();

            var exportsCount = stream.ReadUShortBE();
            Exports = new GlobalRef[exportsCount];
            for (int i = 0; i < exportsCount; i++)
            {
                var address = (ushort)stream.Position;
                var exportOffset = stream.ReadUShortBE();
                if (exportOffset != 0)
                    Exports[i] = new GlobalRef(this, address, exportOffset) { CanBeInvalid = true, Source = "EXPORT" };
            }

            foreach (var obj in _heap.Objects)
                obj.ReadScript(this, stream);

            Operators = Code.Read(this, data, (ushort)stream.Position, (int)(heapPointersOffset - stream.Position));

            // Fill methods body
            /*var methods = Objects.SelectMany(o => o.Methods).OrderBy(m => m.Address).ToArray();
            for (int i = 0; i < methods.Length; i++)
            {
                var m = methods[i];
                ushort end;
                if (i < methods.Length - 1)
                    end = methods[i + 1].Address;
                else
                    end = heapPointersOffset;

                m.ReadBody(data, end);
            }*/

            stream.Seek(heapPointersOffset, SeekOrigin.Begin);
            int count = stream.ReadUShortBE();
            HeapPointers = new BaseElement[count];
            for (int i = 0; i < HeapPointers.Length; i++)
            {
                var pos = stream.Position;
                var offset = stream.ReadUShortBE();
                if (offset != 0)
                {
                    var el = GetElement(offset);
                    HeapPointers[i] = new GlobalRef(this, (ushort)pos, offset) { Source = "HEAP" };
                }
            }

            foreach (var el in AllElements)
                el.SetupByOffset();
        }

        public override byte[] GetBytes()
        {
            ByteBuilder bb = new();

            bb.AddUShortBE(0); // heapOffset
            bb.AddIntBE(_unknown);
            bb.AddUShortBE((ushort)Exports.Length);
            foreach (var exp in Exports)
            {
                if (exp != null)
                    exp.Write(bb);
                else
                    bb.AddShortBE(0);
            }

            foreach (var obj in Objects)
                obj.WriteScriptHead(bb);

            foreach (var op in Operators)
                op.Write(bb);

            /*var methods = Objects.SelectMany(o => o.Methods).OrderBy(m => m.Address).ToArray();
            foreach (var m in methods)
            {
                m.Address = (ushort)bb.Position;
                m.Write(bb);
            }*/

            bb.SetUShortBE(0, (ushort)bb.Position);
            bb.AddUShortBE((ushort)HeapPointers.Length);
            foreach (var p in HeapPointers)
                p.Write(bb);

            foreach (var el in AllElements)
                el.WriteOffset(bb);

            return bb.GetArray();
        }

        public Object1 GetObject(ushort id)
        {
            foreach (var obj in Objects)
                if (obj.ClassId == id) return obj;
            return null;
        }
    }
}
