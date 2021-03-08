namespace SCI_Lib.Decompression
{
    class DecompressorLZW1View : DecompressorLZW1
    {
        protected override void GoUnpack()
        {
            base.GoUnpack();
            _dest = ViewDecoder.DecodeView(_dest);
        }
    }
}
