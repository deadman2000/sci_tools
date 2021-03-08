namespace SCI_Lib.Resources.Scripts.Elements
{
    class Argument
    {
        private int _offset;
        private int _val;
        private byte _size;

        public Argument(int off, byte size)
            : this(off, size, 0)
        {
        }

        public Argument(int off, byte size, int val)
        {
            _offset = off;
            _val = val;
            _size = size;
        }

        public int Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        public int Value
        {
            get { return _val; }
            set { _val = value; }
        }

        public byte Size
        {
            get { return _size; }
            set { _size = value; }
        }

        private BaseElement _ref;

        public BaseElement Reference
        {
            get { return _ref; }
            set { _ref = value; }
        }

    }
}
