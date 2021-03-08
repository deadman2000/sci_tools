using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace SCI_Lib.Resources.Scripts1_1
{
    public class Object1_1
    {
        private Script1_1 _script;

        public Object1_1(Script1_1 script)
        {
            _script = script;
        }

        public string Name { get; set; }

        public override string ToString() => Name;

        public void Read(MemoryStream stream, MemoryStream heap)
        {
            var numVars = heap.ReadUShortBE();
            var varOffset = heap.ReadUShortBE();
            var methodsOffset = heap.ReadUShortBE();
            var mystery = heap.ReadUShortBE();
            if (mystery != 0) throw new Exception("Wrong script format");

            stream.Seek(varOffset, SeekOrigin.Begin);
            ushort[] propSelectors = new ushort[numVars];
            for (int i = 0; i < numVars; i++)
            {
                propSelectors[i] = stream.ReadUShortBE();
            }

            List<LocalVar> propertyValues = new List<LocalVar>();
            for (int i = 0; i < numVars; i++)
            {
                if (i >= 5)
                {
                    bool isString = _script.StringOffsets.Contains((ushort)heap.Position);
                    var w = heap.ReadUShortBE();
                    propertyValues.Add(new LocalVar { Value = w, IsObjectOrString = isString });
                }
                else
                {
                    propertyValues.Add(new LocalVar { Value = 0, IsObjectOrString = false });
                }
            }

            var pos = heap.Position;
            heap.Seek(propertyValues[8].Value, SeekOrigin.Begin);
            Name = heap.ReadString(_script.Package.GameEncoding);
            heap.Seek(pos, SeekOrigin.Begin);

            stream.Seek(methodsOffset, SeekOrigin.Begin);
            var numMethods = stream.ReadUShortBE();
            ushort[] functionSelectors = new ushort[numMethods];
            ushort[] functionOffsets = new ushort[numMethods];
            for (int i = 0; i < numMethods; i++)
            {
                functionSelectors[i] = stream.ReadUShortBE();
                functionOffsets[i] = stream.ReadUShortBE();
            }
        }
    }
}
