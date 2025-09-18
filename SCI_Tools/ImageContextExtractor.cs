using SCI_Lib;
using SCI_Lib.Analyzer;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts1;
using SCI_Lib.Resources.View;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SCI_Tools
{
    internal class ImageContextExtractor
    {
        private SCIPackage _package;

        public ImageContextExtractor(SCIPackage package)
        {
            _package = package;
        }

        public void ExtractAll(string destFolder)
        {
            foreach (var resScript in _package.GetResources<ResScript>())
            {
                try
                {
                    Extract(resScript.Number, destFolder);
                }
                catch (Exception ex)
                {
                    File.AppendAllText(Path.Combine(destFolder, "error.txt"), ex.ToString());
                }
            }
        }

        public void Extract(ushort number, string destFolder)
        {
            var resScript = _package.GetResource<ResScript>(number);
            var script = resScript.GetScript() as Script1;

            foreach (var obj in script.Objects)
            {
                obj.Prepare();
            }
            var room = script.Objects.FirstOrDefault(o => o.Super?.Name == "FPRoom");
            if (room == null) return;
            var picNumber = room.GetProperty("picture");
            if (picNumber == 0) return;

            var pic = _package.GetResource<ResPicture>(picNumber).GetPicture();

            var bgr = pic.GetBackground();

            var builder = pic.CreateImageBuilder();
            builder.Build();

            HashSet<string> complete = new();

            var analyzer = script.Analyze();
            analyzer.Optimize();

            foreach (var proc in analyzer.Procedures)
            {
                if (proc.Name == "init")
                {
                    Console.WriteLine(proc.ClassName);
                    var tree = proc.Optimize();
                    foreach (var node in tree.Nodes)
                    {
                        foreach (var expr in node.Expressions)
                        {
                            if (expr is not CallExpr call) continue;
                            if (call.Method != "setOnMeCheck") continue;

                            var target = (Object1)((RefExpr)call.Target).Ref;
                            var type = ((ConstExpr)call.Args[0]).Value;

                            if (complete.Contains(target.Name)) continue;

                            var path = Path.Combine(destFolder, $"{number}.{target}.png");
                            if (File.Exists(path)) continue;

                            Console.WriteLine($"  {target}");

                            if (type == 1)
                            {
                                int a2 = ((ConstExpr)call.Args[1]).Value & 0xffff;

                                var control = (int)Math.Log2(a2);
                                var color = Palette.EGA.Colors[control];

                                var x = target.GetProperty("x");
                                var y = target.GetProperty("y");

                                var result = HighlightByMask(bgr, builder.Control, color, x, y);
                                result.Save(path);
                                complete.Add(target.Name);
                            }
                            else if (type == 26505)
                            {
                            }
                            else
                            {
                                Console.WriteLine($"     {type} !!!");
                            }
                        }
                    }
                }
            }

            foreach (var obj in script.Objects)
            {
                if (complete.Contains(obj.Name)) continue;
                if (!obj.HasProperty("noun")) continue;
                var noun = obj.GetProperty("noun");
                if (noun == 0) continue;

                var path = Path.Combine(destFolder, $"{number}.{obj.Name}.png");
                if (File.Exists(path)) continue;

                if (!obj.HasProperty("nsLeft")) continue;

                var left = obj.GetPropertySigned("nsLeft");
                var top = obj.GetPropertySigned("nsTop");
                var bottom = obj.GetPropertySigned("nsBottom");
                var right = obj.GetPropertySigned("nsRight");

                if (bottom != 0 && right != 0)
                {
                    var result = HighlightRect(bgr, left, right, top, bottom);
                    result.Save(path);
                }

                if (obj.HasProperty("view"))
                {
                    var view = obj.GetProperty("view");
                    if (view != 0 && view != ushort.MaxValue)
                    {
                        var v = _package.GetResource<ResView>(view).GetView();

                        var loop = (int)obj.GetProperty("loop");
                        if (loop > v.Loops.Count - 1) loop = v.Loops.Count - 1;
                        var l = v.Loops[loop];
                        var c = l.Cells[0];
                        c.GetImage().Save(path);
                    }
                }
            }
        }

        private Image HighlightRect(Image image, int left, int right, int top, int bottom)
        {
            var bitmap = new Bitmap(image);

            const double K = 0.2;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var p = new Point(x, y);
                    if (x < left || x > right || y < top || y > bottom)
                    {
                        var c = bitmap.GetPixel(x, y);
                        bitmap.SetPixel(x, y, Color.FromArgb((int)(c.R * K), (int)(c.G * K), (int)(c.B * K)));
                    }
                }
            }

            return bitmap;
        }

        private Image HighlightByMask(Image source, Bitmap mask, Color maskColor, int x, int y)
        {
            var bitmap = new Bitmap(source);

            var p = GetNearestPoint(mask, maskColor, x, y);
            var pixels = GetRegion(mask, p);

            return HighlightRegion(bitmap, pixels);
        }

        private Image HighlightRegion(Bitmap image, HashSet<Point> pixels)
        {
            const double K = 0.2;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var p = new Point(x, y);
                    if (!pixels.Contains(p))
                    {
                        var c = image.GetPixel(x, y);
                        image.SetPixel(x, y, Color.FromArgb((int)(c.R * K), (int)(c.G * K), (int)(c.B * K)));
                    }
                }
            }

            return image;
        }

        private Point GetNearestPoint(Bitmap image, Color color, int x, int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x >= image.Width) x = image.Width - 1;
            if (y >= image.Height) y = image.Height - 1;

            if (image.GetPixel(x, y) == color)
            {
                return new Point(x, y);
            }

            var bestDist = -1.0;
            int bestX = 0, bestY = 0;

            for (int ix = 0; ix < image.Width; ix++)
            {
                for (int iy = 0; iy < image.Height; iy++)
                {
                    var c = image.GetPixel(ix, iy);
                    if (c == color)
                    {
                        var d = Math.Pow(ix - x, 2) + Math.Pow(iy - y, 2);
                        if (bestDist < 0 || bestDist > d)
                        {
                            bestDist = d;
                            bestX = ix;
                            bestY = iy;
                        }
                    }
                }
            }

            return new Point(bestX, bestY);
        }


        private HashSet<Point> GetRegion(Bitmap bitmap, Point pt)
        {
            HashSet<Point> result = new();
            Stack<Point> pixels = new Stack<Point>();
            var color = bitmap.GetPixel(pt.X, pt.Y);
            pixels.Push(pt);

            while (pixels.Count > 0)
            {
                Point a = pixels.Pop();
                if (a.X < bitmap.Width && a.X > -1 && a.Y < bitmap.Height && a.Y > -1)
                {
                    if (bitmap.GetPixel(a.X, a.Y) == color)
                    {
                        bitmap.SetPixel(a.X, a.Y, color);

                        PushIfNotAdd(a.X - 1, a.Y);
                        PushIfNotAdd(a.X + 1, a.Y);
                        PushIfNotAdd(a.X, a.Y - 1);
                        PushIfNotAdd(a.X, a.Y + 1);
                    }
                }
            }

            return result;

            void PushIfNotAdd(int x, int y)
            {
                if (x < 0 || y < 0) return;
                if (x >= bitmap.Width || y >= bitmap.Height) return;
                Point p = new Point(x, y);
                if (result.Contains(p)) return;

                if (bitmap.GetPixel(x, y) == color)
                {
                    pixels.Push(p);
                    result.Add(p);
                }
            }
        }

    }
}
