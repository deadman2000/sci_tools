using System.Drawing;

namespace SCI_Lib.Resources.Picture
{
    public abstract class SCIPicture
    {
        public abstract byte[] GetBytes();

        public abstract Image GetBackground();

        public abstract void SetBackground(Bitmap bmp);

        public abstract void SetBackgroundIndexed(Bitmap bmp);
    }
}
