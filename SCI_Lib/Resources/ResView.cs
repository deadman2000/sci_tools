using SCI_Lib.Resources.View;
using System;

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

            if (Package.ViewFormat == ViewFormats.NotSet)
            {
                DetectFormat(data);
            }

            switch (Package.ViewFormat)
            {
                case ViewFormats.VGA: view.ReadVGA(data); break;
                case ViewFormats.VGA1_1: view.ReadVGA11(data); break;
                default: throw new NotImplementedException($"Read view format {Package.ViewFormat} is not implemented");
            }

            return view;
        }

        public void SetView(SCIView view)
        {
            _view = view;
        }

        public override byte[] GetPatch()
        {
            if (_view == null) throw new Exception("Patch view is not set");

            switch (Package.ViewFormat)
            {
                case ViewFormats.VGA: return _view.GetBytesVGA();
                case ViewFormats.VGA1_1: return _view.GetBytesVGA11();
                default: throw new NotImplementedException($"Write view format {Package.ViewFormat} is not implemented");
            }
        }

        private void DetectFormat(byte[] data)
        {
            try
            {
                var view = new SCIView(Package);
                view.ReadVGA(data);
                Package.ViewFormat = ViewFormats.VGA;
                return;
            }
            catch
            {
            }

            try
            {
                var view = new SCIView(Package);
                view.ReadVGA11(data);
                Package.ViewFormat = ViewFormats.VGA1_1;
                return;
            }
            catch
            {
            }

            Package.ViewFormat = ViewFormats.Unknown;
        }
    }
}
