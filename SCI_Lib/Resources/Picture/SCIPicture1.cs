using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace SCI_Lib.Resources.Picture
{
    public class SCIPicture1 : SCIPicture
    {
        private readonly PicVector _vector;
        private PicImage Image => _vector.Image;
        private PicPalette Palette => _vector.Palette;

        public byte[] ImageData => Image?.Image;


        public SCIPicture1(byte[] data)
        {
            using var stream = new MemoryStream(data);
            _vector = PicVector.Read(stream);
        }

        public override byte[] GetBytes() => _vector.GetBytes();

        public override Image GetBackground()
        {
            if (Image == null) return null;

            var bmp = new Bitmap(Image.Width, Image.Height, PixelFormat.Format8bppIndexed);
            var pal = bmp.Palette;
            for (int i = 0; i < 256; i++)
                pal.Entries[i] = Palette.Colors[i].GetColor();

            bmp.Palette = pal;

            var data = bmp.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            IntPtr scan0 = data.Scan0;
            Marshal.Copy(Image.Image, 0, scan0, Image.Image.Length);

            bmp.UnlockBits(data);

            return bmp;
        }

        public override void SetBackground(Bitmap bmp)
        {
            if (bmp.Width != Image.Width || bmp.Height != Image.Height)
                throw new ArgumentException("Different image size");

            for (int x = 0; x < bmp.Width; x++)
                for (int y = 0; y < bmp.Height; y++)
                    Image.Image[x + y * Image.Width] = Palette.GetColorIndex(bmp.GetPixel(x, y));
        }

        public override void SetBackgroundIndexed(Bitmap bmp)
        {
            if (bmp.Width != Image.Width || bmp.Height != Image.Height)
                throw new ArgumentException("Different image size");

            if (bmp.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new ArgumentException($"Wrong image pixel format {bmp.PixelFormat}");

            var data = bmp.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            var scan0 = data.Scan0;
            Marshal.Copy(scan0, Image.Image, 0, Image.Image.Length);

            bmp.UnlockBits(data);
        }
    }
}
