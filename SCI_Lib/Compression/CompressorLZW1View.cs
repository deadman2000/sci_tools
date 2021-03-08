using System.IO;

namespace SCI_Lib.Compression
{
    class CompressorLZW1View : CompressorLZW1
    {
        public override void Pack(byte[] data, Stream stream)
        {
            base.Pack(ViewEncoder.EncodeView(data), stream);
        }
    }
}
