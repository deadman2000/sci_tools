using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

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

        public void ShiftLeft(byte c)
        {
            for (int x = 0; x < Width - 1; x++)
                for (int y = 0; y < Height; y++)
                    _pixelMap[x, y] = _pixelMap[x + 1, y];

            for (int y = 0; y < Height; y++)
                _pixelMap[Width - 1, y] = c;
        }

        public void ShiftRight(byte c)
        {
            for (int x = Width - 1; x > 0; x--)
                for (int y = 0; y < Height; y++)
                    _pixelMap[x, y] = _pixelMap[x - 1, y];

            for (int y = 0; y < Height; y++)
                _pixelMap[0, y] = c;
        }

        public void ShiftUp(byte c)
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height - 1; y++)
                    _pixelMap[x, y] = _pixelMap[x, y + 1];

            for (int x = 0; x < Width; x++)
                _pixelMap[x, Height - 1] = c;
        }

        public void ShiftDown(byte c)
        {
            for (int x = 0; x < Width; x++)
                for (int y = Height - 1; y > 0; y--)
                    _pixelMap[x, y] = _pixelMap[x, y - 1];

            for (int x = 0; x < Width; x++)
                _pixelMap[x, 0] = c;
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

        public void ExportToImage(string imagePath)
        {
            var bitmap = new Bitmap(Width, Height);
            var g = Graphics.FromImage(bitmap);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);
            Draw(bitmap, 0, 0);
            bitmap.Save(imagePath);
        }
    }
}
