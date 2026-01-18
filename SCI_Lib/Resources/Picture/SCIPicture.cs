using System.Drawing;

namespace SCI_Lib.Resources.Picture
{
    public abstract class SCIPicture
    {
        protected PicVector _vector;

        public abstract int Width { get; }

        public abstract int Height { get; }

        public PicVector Vector => _vector;

        public abstract byte[] GetBytes();

        public abstract Image GetBackground();

        public abstract void SetBackground(Bitmap bmp);

        public abstract void SetBackground(Bitmap bmp, int[] excludeColors);

        public abstract void SetBackgroundIndexed(Bitmap bmp);

        public abstract Color[] GetPalette();

        public abstract byte[] GetPixels();

        public abstract void SetPixels(byte[] pixels);


        public ImageBuilder CreateImageBuilder() => new ImageBuilder(Width, Height, _vector);
    }
}
