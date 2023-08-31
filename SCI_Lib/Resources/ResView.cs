using SCI_Lib.Resources.View;
using SCI_Lib.SCI0;
using System;
using System.IO;

namespace SCI_Lib.Resources
{
    public class ResView : Resource
    {
        private SCIView _view; // Patched view

        public SCIView GetView()
        {
            var data = GetContent();
            return GetView(data);
        }

        public SCIView GetView(byte[] data)
        {
            var view = new SCIView(Package);

            switch (Package.ViewFormat)
            {
                case ViewFormat.EGA: view.ReadEGA(data); break;
                case ViewFormat.VGA: view.ReadVGA(data); break;
                case ViewFormat.VGA1_1: view.ReadVGA11(data); break;
                default: throw new NotImplementedException($"Read view format {Package.ViewFormat} is not implemented");
            }

            return view;
        }

        public void SetView(SCIView view)
        {
            _view = view;
        }

        protected override void WriteHeader(Stream stream)
        {
            if (Package is SCI0Package)
            {
                base.WriteHeader(stream);
            }
            else
            {
                var header = new byte[26];
                header[0] = (byte)Type;
                header[1] = 0x80;
                stream.Write(header);
            }
        }

        public override byte[] GetPatch()
        {
            if (_view == null) return GetContent();

            return Package.ViewFormat switch
            {
                ViewFormat.VGA => _view.GetBytesVGA(),
                ViewFormat.VGA1_1 => _view.GetBytesVGA11(),
                _ => throw new NotImplementedException($"Write view format {Package.ViewFormat} is not implemented"),
            };
        }
    }
}
