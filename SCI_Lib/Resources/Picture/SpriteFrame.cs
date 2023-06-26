using System;
using System.Drawing;

namespace SCI_Lib.Pictures
{
    public class SpriteFrame
    {
        private byte[,] _pixelMap;

        public SpriteFrame(int width, int height)
        {
            if (width <= 0 || height <= 0) throw new Exception("Invalid frame size");

            Width = width;
            Height = height;
            _pixelMap = new byte[width, height];
        }

        public SpriteFrame(SpriteFrame original)
        {
            Width = original.Width;
            Height = original.Height;
            _pixelMap = new byte[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    _pixelMap[x, y] = original._pixelMap[x, y];
        }

        public byte this[int x, int y]
        {
            get { return _pixelMap[x, y]; }
            set { _pixelMap[x, y] = value; }
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public void Resize(int w, int h)
        {
            byte[,] newMap = new byte[w, h];

            for (int y = 0; y < h && y < Height; y++)
                for (int x = 0; x < w && x < Width; x++)
                    newMap[x, y] = _pixelMap[x, y];

            _pixelMap = newMap;
            Width = w;
            Height = h;
        }

        public void ShiftLeft(byte color)
        {
            for (int x = 0; x < Width - 1; x++)
                for (int y = 0; y < Height; y++)
                    _pixelMap[x, y] = _pixelMap[x + 1, y];

            for (int y = 0; y < Height; y++)
                _pixelMap[Width - 1, y] = color;
        }

        public void ShiftRight(byte color)
        {
            for (int x = Width - 1; x > 0; x--)
                for (int y = 0; y < Height; y++)
                    _pixelMap[x, y] = _pixelMap[x - 1, y];

            for (int y = 0; y < Height; y++)
                _pixelMap[0, y] = color;
        }

        public void ShiftUp(byte color)
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height - 1; y++)
                    _pixelMap[x, y] = _pixelMap[x, y + 1];

            for (int x = 0; x < Width; x++)
                _pixelMap[x, Height - 1] = color;
        }

        public void ShiftDown(byte color)
        {
            for (int x = 0; x < Width; x++)
                for (int y = Height - 1; y > 0; y--)
                    _pixelMap[x, y] = _pixelMap[x, y - 1];

            for (int x = 0; x < Width; x++)
                _pixelMap[x, 0] = color;
        }

        public void Draw(Bitmap bitmap, int tx, int ty)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (this[x, y] == 1)
                        bitmap.SetPixel(tx + x, ty + y, Color.Black);
                }
            }
        }

        public Image GetImage()
        {
            var bitmap = new Bitmap(Width, Height);
            var g = Graphics.FromImage(bitmap);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);
            Draw(bitmap, 0, 0);
            g.Dispose();
            return bitmap;
        }

        public void ExportToImage(string imagePath)
        {
            GetImage().Save(imagePath);
        }

        public SpriteFrame GetOutline()
        {
            if (Height <= 1) return new SpriteFrame(this);

            var th = Height;
            var tw = Width;

            // Check right
            for (int y = 0; y < Height; y++)
                if (_pixelMap[Width - 1, y] > 0)
                {
                    tw++;
                    break;
                }

            // Check bottom
            for (int x = 0; x < Width; x++)
                if (_pixelMap[x, Height - 1] > 0)
                {
                    th++;
                    break;
                }

            var frame = new SpriteFrame(tw, th);

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (_pixelMap[x, y] > 0)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (dx == 0 && dy == 0) continue;
                                var tx = x + dx;
                                var ty = y + dy;
                                if (tx < 0 || ty < 0 || tx == tw || ty == th) continue;
                                //if (_pixelMap[tx, ty] > 0) continue;

                                frame._pixelMap[tx, ty] = 1;
                            }
                    }

            return frame;
        }

        public void MirrorHoriz()
        {
            for (int x = 0; x < Width / 2; x++)
                for (int y = 0; y < Height; y++)
                {
                    var x2 = Width - x - 1;
                    (_pixelMap[x, y], _pixelMap[x2, y]) = (_pixelMap[x2, y], _pixelMap[x, y]);
                }
        }
    }
}
