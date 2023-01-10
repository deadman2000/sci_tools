using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SCI_Lib.Resources.Picture
{
    public abstract class SCIPicture
    {
        public abstract byte[] GetBytes();

        public abstract Image GetBackground();

        public abstract void SetBackground(Bitmap bmp);
    }
}
