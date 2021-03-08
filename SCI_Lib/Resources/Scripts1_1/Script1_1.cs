using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace SCI_Lib.Resources.Scripts1_1
{
    public class Script1_1
    {
        public Script1_1(Resource res)
        {
            Resource = res;
            SourceData = res.GetContent();
            ReadScriptSCI1_1();
        }

        public Resource Resource { get; }

        public SCIPackage Package => Resource.Package;

        public byte[] SourceData { get; }

        public List<Object1_1> Objects { get; set; } = new List<Object1_1>();

        public HashSet<ushort> StringOffsets { get; set; }

        public List<string> Strings { get; set; } = new List<string>();

        private void ReadScriptSCI1_1()
        {
            // https://github.com/scummvm/scummvm/blob/master/engines/sci/engine/script.cpp#L390
            // https://github.com/icefallgames/SCICompanion/blob/master/SCICompanionLib/Src/Compile/CompiledScript.cpp#L226
            List<ushort> exportsTO = new List<ushort>();
            List<ushort> exportedObjInst = new List<ushort>();
            var heapRes = Package.GetResouce(ResType.Heap, Resource.Volumes[0].Num);

            using var stream = new MemoryStream(SourceData.Length);
            stream.Write(SourceData, 0, SourceData.Length);
            stream.Seek(0, SeekOrigin.Begin);

            var endOfStringOffset = stream.ReadUShortBE();
            var heapPoints = ReadOffsets(stream, endOfStringOffset);
            stream.Seek(4, SeekOrigin.Current);

            var exportsCount = stream.ReadUShortBE();
            for (int i = 0; i < exportsCount; i++)
            {
                var isHeapPointer = heapPoints.Contains((ushort)stream.Position);
                var exportOffset = stream.ReadUShortBE();

                exportsTO.Add(exportOffset);
                if (isHeapPointer)
                    exportedObjInst.Add(exportOffset);
            }

            List<LocalVar> localVars = new List<LocalVar>();
            var heapData = heapRes.GetContent();
            using var heap = new MemoryStream(heapData);

            var stringOffset = heap.ReadUShortBE();
            StringOffsets = ReadOffsets(heap, stringOffset);
            var localsCount = heap.ReadUShortBE();
            for (int i = 0; i < localsCount; i++)
            {
                bool isObjectOrString = StringOffsets.Contains((ushort)heap.Position);
                ushort w = heap.ReadUShortBE();
                localVars.Add(new LocalVar { Value = w, IsObjectOrString = isObjectOrString });
            }


            while (true)
            {
                var magic = heap.ReadUShortBE();
                if (magic == 0)
                    break;

                if (magic != 0x1234) throw new Exception("Wrong script format");

                var obj = new Object1_1(this);
                obj.Read(stream, heap);
                Objects.Add(obj);
            }

            do
            {
                var offs = (ushort)heap.Position;
                var str = heap.ReadString(Resource.GameEncoding);
                List<ushort> stringsOffset = new List<ushort>();
                if (str.Length > 0 || heap.Position < stringOffset)
                {
                    stringsOffset.Add(offs);
                    Strings.Add(str);
                }
            }
            while (heap.Position < stringOffset);
        }

        HashSet<ushort> ReadOffsets(MemoryStream stream, ushort offset)
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
    }
}
