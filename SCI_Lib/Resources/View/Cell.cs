using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SCI_Lib.Resources.View
{
    public class Cell
    {
        public Cell(SCIView view)
        {
            View = view;
        }

        public SCIView View { get; }

        public ushort Width { get; set; }

        public ushort Height { get; set; }

        public short X { get; set; }

        public short Y { get; set; }

        public byte TransparentColor { get; set; }

        public byte[] Pixels { get; set; }

        public Image GetImage()
        {
            Palette palette = View.Palette != null ? View.Palette : View.Package.GlobalPalette;

            var bmp = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
            var pal = bmp.Palette;
            for (int i = 0; i < 256; i++)
                pal.Entries[i] = palette.Colors[i];

            bmp.Palette = pal;

            var data = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            IntPtr scan0 = data.Scan0;
            for (int y = 0; y < Height; y++)
            {
                Marshal.Copy(Pixels, y * Width, scan0, Width);
                scan0 += data.Stride;
            }

            bmp.UnlockBits(data);

            return bmp;
        }
    }
}
