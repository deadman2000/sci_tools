using SCI_Lib.Resources.View;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SCI_Lib.Resources.Picture
{
    public class ImageBuilder
    {
        private readonly int _width;
        private readonly int _height;
        private readonly PicVector _vector;
        private Layer _visual;
        private Layer _control;
        private Layer _priority;

        class Layer
        {
            public Bitmap bitmap;
            public Graphics graphics;
            private Color[] palette;
            public Color color;
            public bool enabled;

            public Layer(string name, int width, int height, Color[] palette)
            {
                Name = name;
                bitmap = new Bitmap(width, height);
                graphics = Graphics.FromImage(bitmap);
                graphics.Clear(palette[0]);
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                this.palette = palette;
            }
            
            public string Name { get; set; }

            public void SetColor(int index)
            {
                color = palette[index];
                enabled = true;
            }

            public void Disable()
            {
                enabled = false;
            }

            public void Fill(Point p)
            {
                if (!enabled) return;
                FloodFill(p, color);
            }

            private void FloodFill(Point pt, Color color)
            {
                Stack<Point> pixels = new();
                var targetColor = bitmap.GetPixel(pt.X, pt.Y);
                if (targetColor == color) return;
                pixels.Push(pt);

                while (pixels.Count > 0)
                {
                    Point a = pixels.Pop();
                    if (a.X < bitmap.Width && a.X > -1 && a.Y < bitmap.Height && a.Y > -1)
                    {
                        if (bitmap.GetPixel(a.X, a.Y) == targetColor)
                        {
                            bitmap.SetPixel(a.X, a.Y, color);

                            PushIfTarget(a.X - 1, a.Y);
                            PushIfTarget(a.X + 1, a.Y);
                            PushIfTarget(a.X, a.Y - 1);
                            PushIfTarget(a.X, a.Y + 1);
                        }
                    }
                }

                void PushIfTarget(int x, int y)
                {
                    if (x < 0 || y < 0) return;
                    if (x >= bitmap.Width || y >= bitmap.Height) return;
                    if (bitmap.GetPixel(x, y) == targetColor)
                        pixels.Push(new Point(x, y));
                }
            }

            public void DrawLine(Point from, Point to)
            {
                if (!enabled) return;
                graphics.DrawLine(new Pen(color), from, to);
            }
        }

        public ImageBuilder(int width, int height, PicVector vector)
        {
            _width = width;
            _height = height;
            _vector = vector;
        }

        public Bitmap Visual => _visual.bitmap;
        public Bitmap Control => _control.bitmap;
        public Bitmap Priority => _priority.bitmap;

        public void Build()
        {
            _visual = new Layer("Visual", _width, _height, Palette.EGA.Colors);
            _control = new Layer("Control", _width, _height, Palette.EGA.Colors);
            _priority = new Layer("Priority", _width, _height, Palette.EGA.Colors);

            Layer[] layers = new[] { _visual, _control, _priority };
            foreach (var cmd in _vector.Commands)
            {
                switch (cmd.OpCode)
                {
                    case PicOpCode.RELATIVE_SHORT_LINES:
                        {
                            var p = GetPoint(cmd.Args, 0);
                            for (int i = 3; i < cmd.Args.Length; i++)
                            {
                                var from = p;

                                var b = cmd.Args[i];
                                if ((b & 0x80) != 0)
                                    p = p with { X = p.X - ((b >> 4) & 7) };
                                else
                                    p = p with { X = p.X + (b >> 4) };

                                if ((b & 0x08) != 0)
                                    p = p with { Y = p.Y - (b & 7) };
                                else
                                    p = p with { Y = p.Y + (b & 7) };

                                foreach (var layer in layers) layer.DrawLine(from, p);
                            }
                        }
                        break;
                    case PicOpCode.RELATIVE_MEDIUM_LINES:
                        {
                            var p = GetPoint(cmd.Args, 0);
                            for (int i = 3; i < cmd.Args.Length; i += 2)
                            {
                                var from = p;

                                var b = cmd.Args[i];
                                if ((b & 0x80) != 0)
                                    p = p with { Y = p.Y - (b & 0x7f) };
                                else
                                    p = p with { Y = p.Y + b };

                                 b = cmd.Args[i+1];
                                if ((b & 0x80) != 0)
                                    p = p with { X = p.X - 128 + (b & 0x7f) };
                                else
                                    p = p with { X = p.X + b };

                                foreach (var layer in layers) layer.DrawLine(from, p);
                            }
                        }
                        break;
                    case PicOpCode.RELATIVE_LONG_LINES:
                        {
                            var p = GetPoint(cmd.Args, 0);
                            for (int i = 3; i < cmd.Args.Length; i += 3)
                            {
                                var from = p;
                                p = GetPoint(cmd.Args, i);
                                foreach (var layer in layers) layer.DrawLine(from, p);
                            }
                        }
                        break;

                    case PicOpCode.FILL:
                        {
                            var p = GetPoint(cmd.Args, 0);
                            foreach (var layer in layers) layer.Fill(p);
                        }
                        break;

                    case PicOpCode.SET_CONTROL:
                        _control.SetColor(cmd.Args[0] & 0x0f);
                        break;
                    case PicOpCode.DISABLE_CONTROL:
                        _control.Disable();
                        break;

                    case PicOpCode.SET_VISUAL:
                        _visual.SetColor(cmd.Args[0] & 0x0f);
                        break;
                    case PicOpCode.DISABLE_VISUAL:
                        _visual.Disable();
                        break;

                    case PicOpCode.SET_PRIORITY:
                        _priority.SetColor(cmd.Args[0] & 0x0f);
                        break;
                    case PicOpCode.DISABLE_PRIORITY:
                        _priority.Disable();
                        break;

                    case PicOpCode.SET_PATTERN:
                    case PicOpCode.RELATIVE_PATTERNS:
                    case PicOpCode.ABSOLUTE_PATTERNS:
                    case PicOpCode.RELATIVE_MEDIUM_PATTERNS:
                        break;

                    case PicOpCode.OPX:
                        break;

                    case PicOpCode.END:
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private static Point GetPoint(byte[] data, int index)
        {
            var b = data[index];
            var x = data[index + 1] + ((b & 0xf0) << 4);
            var y = data[index + 2] + ((b & 0xf) << 8);
            return new Point(x, y);
        }
    }
}
