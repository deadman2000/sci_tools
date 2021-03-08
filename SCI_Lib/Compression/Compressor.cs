using SCI_Lib.Resources;
using System.IO;

namespace SCI_Lib.Compression
{
    public abstract class Compressor
    {
        public static bool DEBUG = false;
   
        protected byte[] _data;
        protected Stream _stream;

        public byte[] Pack(byte[] data)
        {
            var mem = new MemoryStream();
            Pack(data, mem);
            return mem.ToArray();
        }

        public virtual void Pack(byte[] data, Stream stream)
        {
            var start = stream.Position;
            _data = data;
            _stream = stream;
            GoPack();

            CompSize = (int)(stream.Position - start);
        }

        public int DecompSize => _data.Length;

        public int CompSize { get; set; }

        protected abstract void GoPack();
    }
}
