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
            sec.Read(data, offset, length);
            return sec;
        }

        static Section CreateObj(SectionType type)
        {
            switch (type)
            {
                case SectionType.Object: return new ObjectSection();
                case SectionType.Code: return new CodeSection();
                case SectionType.String: return new StringSection();
                case SectionType.Class: return new ClassSection();
                case SectionType.Export: return new ExportSection();
                case SectionType.Relocation: return new RelocationSection();
                case SectionType.Preload: return new PreloadTextSection();
                case SectionType.LocalVariables: return new LocalVariablesSection();
                default: throw new NotImplementedException();
            }
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

        public virtual void Write(ByteBuilder bb)
        {
        }

        public virtual void WriteOffsets(ByteBuilder bb)
        {
        }

        public override string ToString()
        {
            return $"{_offset:x4}: {Type}";
        }

        protected static ushort ReadShortBE(byte[] data, ref ushort offset)
        {
            return (ushort)(data[offset++] | (data[offset++] << 8));
        }

    }
}
