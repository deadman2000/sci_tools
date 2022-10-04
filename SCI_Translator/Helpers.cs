using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SCI_Translator
{
    public static class Helpers
    {
        public static Image Rescale(this Image image, int size)
        {
            Bitmap b = new(size, size);
            Graphics g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            var m = Math.Max(image.Width, image.Height);
            var f = (double)size / m;
            var targetW = (int)(image.Width * f);
            var targetH = (int)(image.Height * f);
            var x = (size - targetW) / 2;
            var y = (size - targetH) / 2;

            g.DrawImage(image, x, y, targetW, targetH);
            g.Dispose();
            return b;
        }
    }
}
