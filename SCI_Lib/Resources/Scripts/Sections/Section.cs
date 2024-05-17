using SCI_Lib.Utils;
using System;

namespace SCI_Lib.Resources.Scripts.Sections
{
    public abstract class Section
    {
        public static Section Create(Script script, SectionType type, byte[] data, ushort offset, int length)
        {
            Section sec = CreateObj(type);
            sec._script = script;
            sec._offset = offset;
            sec._type = type;
            sec._size = length;
            if (length > 0)
                sec.Read(data, offset, length);
            return sec;
        }

        static Section CreateObj(SectionType type)
        {
            return type switch
            {
                SectionType.Object => new ObjectSection(),
                SectionType.Code => new CodeSection(),
                SectionType.Synonym => new SynonymSecion(),
                SectionType.Said => new SaidSection(),
                SectionType.String => new StringSection(),
                SectionType.Class => new ClassSection(),
                SectionType.Export => new ExportSection(),
                SectionType.Relocation => new RelocationSection(),
                SectionType.Preload => new PreloadTextSection(),
                SectionType.LocalVariables => new LocalVariablesSection(),
                _ => throw new NotImplementedException(),
            };
        }

        public virtual void SetupByOffset() { }

        protected Script _script;
        protected int _offset;
        protected int _size;
        private SectionType _type;

        public SCIPackage Package => _script.Package;

        public Script Script => _script;

        public SectionType Type => _type;

        public int Address => _offset;

        public int Size => _size;

        public abstract void Read(byte[] data, ushort offset, int length);

        public abstract void Write(ByteBuilder bb);

        public override string ToString()
        {
            return $"{_offset:x4}: {Type}";
        }

        protected static ushort ReadShortBE(byte[] data, ref ushort offset)
        {
            return (ushort)(data[offset++] | (data[offset++] << 8));
        }

        protected static ushort ReadShortLE(byte[] data, ref ushort offset)
        {
            return (ushort)((data[offset++] << 8) | data[offset++]);
        }

    }
}
