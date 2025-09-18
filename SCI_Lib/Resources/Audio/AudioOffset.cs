namespace SCI_Lib.Resources.Audio
{
    public struct AudioOffset
    {
        public uint Number { get; set; }

        public int Offset { get; set; }

        public AudioOffset(uint number, int offset)
        {
            Number = number;
            Offset = offset;
        }

        public override string ToString() => $"{Number:x08}: {Offset:x08}";
    }
}
