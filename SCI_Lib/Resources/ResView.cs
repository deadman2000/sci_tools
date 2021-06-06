using SCI_Lib.Resources.View;
using System;

namespace SCI_Lib.Resources
{
    public class ResView : Resource
    {
        public SCIView GetView()
        {
            var data = GetContent();
            var view = new SCIView(Package);

            if (Package.ViewFormat == ViewFormats.NotSet)
            {
                DetectFormat(data);
            }

            switch (Package.ViewFormat)
            {
                case ViewFormats.VGA: view.ReadVGA(data); break;
                case ViewFormats.VGA1_1: view.ReadVGA11(data); break;
                case ViewFormats.Unknown: throw new NotImplementedException();
            }

            return view;
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
