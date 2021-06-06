using SCI_Lib.Utils;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SCI_Lib.Resources.View
{
    public class Palette
    {
        public Color[] Colors { get; set; }

        public Image GetImage()
        {
            int realCount = 0;
            for (int k = Colors.Length - 1; k >= 0; k--)
            {
                if (!Colors[k].IsEmpty)
                {
                    realCount = k + 1;
                    break;
                }
            }

            int height = realCount / 16;
            if (realCount % 16 > 0) height++;

            var bmp = new Bitmap(16, height, PixelFormat.Format24bppRgb);

            int ind = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (ind >= Colors.Length) break;

                    bmp.SetPixel(x, y, Colors[ind]);
                    ind++;
                }
                if (ind >= Colors.Length) break;
            }

            return bmp;
        }
    }
}
