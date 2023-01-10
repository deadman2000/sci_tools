using Microsoft.VisualBasic;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace SCI_Lib.Resources.Scripts1_1
{
    public class Script1_1 : IScript
    {
        private readonly byte[] _sourceData;

        public Resource Resource { get; }

        public SCIPackage Package => Resource.Package;


        public ushort[] ExportsOffsets { get; private set; }

        public bool[] ExportsOffsetsIsHeap { get; private set; }

        public List<Object1_1> Objects { get; } = new List<Object1_1>();

        public List<LocalVar> LocalVars { get; } = new List<LocalVar>();

        public HashSet<ushort> StringOffsets { get; set; }

        public List<string> Strings { get; set; } = new List<string>();

        public Script1_1(Resource res)
        {
            Resource = res;
            _sourceData = res.GetContent();
            ReadScriptSCI1_1();
        }

        private void ReadScriptSCI1_1()
        {
            // https://github.com/scummvm/scummvm/blob/master/engines/sci/engine/script.cpp#L390
            // https://github.com/icefallgames/SCICompanion/blob/master/SCICompanionLib/Src/Compile/CompiledScript.cpp#L226
            using var stream = new MemoryStream(_sourceData);
            //stream.Write(_sourceData, 0, _sourceData.Length);
            //stream.Seek(0, SeekOrigin.Begin);

            var endOfStringOffset = stream.ReadUShortBE();
            var heapPoints = ReadOffsets(stream, endOfStringOffset);
            stream.Seek(4, SeekOrigin.Current);

            var exportsCount = stream.ReadUShortBE();
            ExportsOffsets = new ushort[exportsCount];
            ExportsOffsetsIsHeap = new bool[exportsCount];
            for (int i = 0; i < exportsCount; i++)
            {
                var isHeapPointer = heapPoints.Contains((ushort)stream.Position);
                var exportOffset = stream.ReadUShortBE();

                ExportsOffsets[i] = exportOffset;
                if (isHeapPointer)
                    ExportsOffsetsIsHeap[i] = true;
            }

            var heapRes = Package.GetResource<ResHeap>(Resource.Number);
            //var heap = heapRes.GetHeap();

            var heapData = heapRes.GetContent();
            using var heap = new MemoryStream(heapData);

            var stringOffset = heap.ReadUShortBE();
            StringOffsets = ReadOffsets(heap, stringOffset);
            var localsCount = heap.ReadUShortBE();
            for (int i = 0; i < localsCount; i++)
            {
                bool isObjectOrString = StringOffsets.Contains((ushort)heap.Position);
                ushort w = heap.ReadUShortBE();
                LocalVars.Add(new LocalVar { Value = w, IsObjectOrString = isObjectOrString });
            }


            while (true)
            {
                var offset = (ushort)heap.Position;
                var magic = heap.ReadUShortBE();
                if (magic == 0)
                    break;

                if (magic != 0x1234) throw new Exception("Wrong script format");

                var obj = new Object1_1(this, heapData, offset);
                obj.Read(stream, heap);
                Objects.Add(obj);

                for (int i = 0; i < exportsCount; i++)
                {
                    if (ExportsOffsetsIsHeap[i] && ExportsOffsets[i] == obj.Offset)
                        obj.ExportInd = i;
                }
            }

            do
            {
                var offs = (ushort)heap.Position;
                var str = heap.ReadString(Resource.GameEncoding);
                //List<ushort> stringsOffset = new List<ushort>();
                if (str.Length > 0 || heap.Position < stringOffset)
                {
                    //stringsOffset.Add(offs);
                    Strings.Add(str);
                }
            }
            while (heap.Position < stringOffset);
        }

        private static HashSet<ushort> ReadOffsets(MemoryStream stream, ushort offset)
        {
            var oldPos = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);

            var count = stream.ReadUShortBE();
            var heapPointers = new HashSet<ushort>();
            for (int i = 0; i < count; i++)
                heapPointers.Add(stream.ReadUShortBE());

            stream.Seek(oldPos, SeekOrigin.Begin);
            return heapPointers;
        }

        public byte[] GetBytes()
        {
            throw new NotImplementedException();
        }

        static readonly StringConst[] empty = Array.Empty<StringConst>();
        public IEnumerable<StringConst> AllStrings()
        {
            return empty;
        }

        public IClass GetClass(ushort id)
        {
            foreach (var obj in Objects)
                if (obj.ClassId == id) return obj;
            return null;
        }

        public List<T> Get<T>() where T : Section
        {
            return null;
        }
    }
}
