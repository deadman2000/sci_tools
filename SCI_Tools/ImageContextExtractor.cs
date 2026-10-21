using SCI_Lib;
using SCI_Lib.Analyzer;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Picture;
using SCI_Lib.Resources.Scripts1;
using SCI_Lib.Resources.View;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SCI_Tools
{
    internal class ImageContextExtractor
    {
        private const double Dim = 0.2;

        private readonly SCIPackage _package;

        public ImageContextExtractor(SCIPackage package)
        {
            _package = package;
        }

        public void ExtractAll(string destFolder)
        {
            Directory.CreateDirectory(destFolder);
            foreach (var resScript in _package.GetResources<ResScript>())
            {
                try
                {
                    Extract(resScript.Number, destFolder);
                }
                catch (Exception ex)
                {
                    var message = $"script {resScript.Number}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}";
                    File.AppendAllText(Path.Combine(destFolder, "error.txt"), message);
                    Console.WriteLine(message);
                }
            }
        }

        public void Extract(ushort number, string destFolder)
        {
            Directory.CreateDirectory(destFolder);

            var resScript = _package.GetResource<ResScript>(number);
            if (resScript.GetScript() is not Script1 script) return;

            foreach (var obj in script.Objects)
            {
                try
                {
                    obj.Prepare();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  {number}.{obj}: {ex.Message}");
                }
            }

            var room = script.Objects.FirstOrDefault(IsRoom);
            if (room == null) return;

            var analyzer = script.Analyze();
            var picNumber = ResolvePicture(room, analyzer);
            Bitmap background = null;
            byte[] control = null;
            if (picNumber != 0)
            {
                var pictureRes = _package.GetResource<ResPicture>(picNumber);
                if (pictureRes != null)
                {
                    try
                    {
                        var pic = pictureRes.GetPicture();
                        background = OpenBackground(pic) ?? ComposeViews(script, room, analyzer, pic.Width, pic.Height);
                        if (background != null)
                            control = ReadControl(pic);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  picture {picNumber}: {ex.Message}");
                    }
                }
            }

            var complete = new HashSet<string>();
            if (background == null)
            {
                SaveViews(script, number, destFolder, complete);
                return;
            }

            using (background)
            {
            foreach (var proc in OrderedProcedures(analyzer, room))
            {
                OptimizedTree tree;
                try
                {
                    tree = proc.Optimize();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  {number}.{proc.ClassName}.{proc.Name}: {ex.Message}");
                    continue;
                }

                if (tree.Nodes == null) continue;

                foreach (var node in tree.Nodes)
                {
                    if (node.Expressions == null) continue;
                    foreach (var expr in node.Expressions)
                    {
                        foreach (var call in EnumerateCalls(expr))
                            ApplyOnMeCheck(call, number, destFolder, background, control, complete);
                    }
                }
            }

            foreach (var obj in script.Objects)
            {
                if (string.IsNullOrEmpty(obj.Name) || complete.Contains(obj.Name)) continue;
                if (!obj.HasProperty("noun") || obj.GetProperty("noun") == 0) continue;

                var path = ImagePath(destFolder, number, obj.Name);
                if (File.Exists(path)) continue;

                if (obj.HasProperty("nsLeft"))
                {
                    var left = obj.GetPropertySigned("nsLeft");
                    var top = obj.GetPropertySigned("nsTop");
                    var bottom = obj.GetPropertySigned("nsBottom");
                    var right = obj.GetPropertySigned("nsRight");
                    if (bottom != 0 && right != 0)
                    {
                        using var result = HighlightRect(background, left, right, top, bottom);
                        result.Save(path);
                        complete.Add(obj.Name);
                    }
                }

                SaveView(obj, number, destFolder, complete);
            }
            }
        }

        private static void SaveViews(Script1 script, ushort number, string destFolder, HashSet<string> complete)
        {
            foreach (var obj in script.Objects)
            {
                if (string.IsNullOrEmpty(obj.Name)) continue;
                if (File.Exists(ImagePath(destFolder, number, obj.Name))) continue;
                SaveView(obj, number, destFolder, complete);
            }
        }

        private static void SaveView(Object1 obj, ushort number, string destFolder, HashSet<string> complete)
        {
            if (string.IsNullOrEmpty(obj.Name) || complete.Contains(obj.Name)) return;
            if (!obj.HasProperty("noun") || obj.GetProperty("noun") == 0) return;
            if (!obj.HasProperty("view")) return;

            var view = obj.GetProperty("view");
            if (view == 0 || view == ushort.MaxValue) return;

            var viewRes = obj.Script.Package.GetResource<ResView>(view);
            if (viewRes == null) return;

            var v = viewRes.GetView();
            if (v.Loops == null || v.Loops.Count == 0) return;

            var loop = obj.HasProperty("loop") ? obj.GetProperty("loop") : 0;
            if (loop >= v.Loops.Count) loop = (ushort)(v.Loops.Count - 1);
            var cells = v.Loops[loop].Cells;
            if (cells == null || cells.Count == 0) return;

            var path = ImagePath(destFolder, number, obj.Name);
            using var image = cells[0].GetImage();
            image.Save(path);
            complete.Add(obj.Name);
        }

        private void ApplyOnMeCheck(CallExpr call, ushort number, string destFolder, Bitmap background, byte[] control, HashSet<string> complete)
        {
            if (call.Method != "setOnMeCheck") return;
            if (call.Target is not RefExpr targetRef || targetRef.Ref is not Object1 target) return;
            if (string.IsNullOrEmpty(target.Name) || complete.Contains(target.Name)) return;
            if (call.Args == null || call.Args.Count == 0 || call.Args[0] is not ConstExpr typeExpr) return;

            var path = ImagePath(destFolder, number, target.Name);
            if (File.Exists(path))
            {
                complete.Add(target.Name);
                return;
            }

            if (typeExpr.Value != 1) return;

            var mask = 0;
            for (int i = 1; i < call.Args.Count; i++)
            {
                if (call.Args[i] is ConstExpr bit)
                    mask |= (ushort)bit.Value;
            }
            if (mask == 0 || control == null) return;
            if (!target.HasProperty("x") || !target.HasProperty("y")) return;

            var x = target.GetPropertySigned("x");
            var y = target.GetPropertySigned("y");
            var pixels = ControlRegion(control, background.Width, background.Height, mask, x, y);
            if (pixels.Count == 0) return;

            Console.WriteLine($"  {number}.{target.Name} control {mask}");
            using var result = HighlightRegion(background, pixels);
            result.Save(path);
            complete.Add(target.Name);
        }

        private static ushort ResolvePicture(Object1 room, ScriptAnalyzer analyzer)
        {
            if (room.HasProperty("picture"))
            {
                var stored = room.GetProperty("picture");
                if (stored != 0) return stored;
            }

            foreach (var proc in analyzer.Procedures)
            {
                if (proc.ClassName != room.Name || proc.Name != "init") continue;

                OptimizedTree tree;
                try
                {
                    tree = proc.Optimize();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  {room.Name}.init: {ex.Message}");
                    continue;
                }

                if (tree.Nodes == null) continue;
                foreach (var node in tree.Nodes)
                {
                    if (node.Expressions == null) continue;
                    foreach (var expr in node.Expressions)
                    {
                        var number = FindPictureAssignment(expr);
                        if (number != 0) return number;
                    }
                }
            }

            return 0;
        }

        private static ushort FindPictureAssignment(Expr expr)
        {
            if (expr == null) return 0;
            if (expr is SetExpr set && set.A is ParamExpr param && param.Name == "picture" && set.B is ConstExpr value && value.Value > 0)
                return (ushort)value.Value;

            if (expr.Args == null) return 0;
            foreach (var arg in expr.Args)
            {
                var number = FindPictureAssignment(arg);
                if (number != 0) return number;
            }
            return 0;
        }

        private static Bitmap OpenBackground(SCIPicture pic)
        {
            var source = pic.GetBackground();
            if (source == null) return null;

            var bitmap = new Bitmap(source);
            source.Dispose();
            return bitmap;
        }

        // SCI 1.1 rooms can store only a control map in the picture and paint the
        // visible room from views (addToPic / init) in the room's init.
        private static Bitmap ComposeViews(Script1 script, Object1 room, ScriptAnalyzer analyzer, int width, int height)
        {
            var stamps = new List<Object1>();
            var seen = new HashSet<string>();
            foreach (var proc in analyzer.Procedures)
            {
                if (proc.ClassName != room.Name || proc.Name != "init") continue;

                OptimizedTree tree;
                try
                {
                    tree = proc.Optimize();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  {room.Name}.init: {ex.Message}");
                    continue;
                }

                if (tree.Nodes == null) continue;
                foreach (var node in tree.Nodes)
                {
                    if (node.Expressions == null) continue;
                    foreach (var expr in node.Expressions)
                    {
                        foreach (var call in EnumerateCalls(expr))
                        {
                            if (call.Method is not ("addToPic" or "init")) continue;
                            if (call.Target is not RefExpr targetRef || targetRef.Ref is not Object1 target) continue;
                            if (string.IsNullOrEmpty(target.Name) || !seen.Add(target.Name)) continue;
                            if (!target.HasProperty("view")) continue;
                            var view = target.GetProperty("view");
                            if (view == 0 || view == ushort.MaxValue) continue;
                            var x = target.HasProperty("x") ? target.GetPropertySigned("x") : 0;
                            var y = target.HasProperty("y") ? target.GetPropertySigned("y") : 0;
                            if (x == 0 && y == 0) continue;
                            stamps.Add(target);
                        }
                    }
                }
            }

            if (stamps.Count == 0) return null;

            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
                graphics.Clear(Color.Black);

            foreach (var obj in stamps)
                StampView(bitmap, obj);
            return bitmap;
        }

        private static void StampView(Bitmap dest, Object1 obj)
        {
            var viewRes = obj.Script.Package.GetResource<ResView>(obj.GetProperty("view"));
            if (viewRes == null) return;

            var view = viewRes.GetView();
            if (view.Loops == null || view.Loops.Count == 0) return;

            var loop = obj.HasProperty("loop") ? obj.GetProperty("loop") : 0;
            if (loop >= view.Loops.Count) loop = (ushort)(view.Loops.Count - 1);
            var cells = view.Loops[loop].Cells;
            if (cells == null || cells.Count == 0) return;

            var cel = obj.HasProperty("cel") ? obj.GetProperty("cel") : 0;
            if (cel >= cells.Count) cel = (ushort)(cells.Count - 1);
            var cell = cells[cel];

            // SCI 1.1 view cels store an origin, not a top-left displace.
            // ScummVM: dx = width/2 - originX, dy = height - originY - 1.
            var objX = obj.HasProperty("x") ? obj.GetPropertySigned("x") : 0;
            var objY = (obj.HasProperty("y") ? obj.GetPropertySigned("y") : 0)
                - (obj.HasProperty("z") ? obj.GetPropertySigned("z") : 0);
            var x = objX - (cell.Width / 2 - cell.X);
            var y = objY - (cell.Height - cell.Y - 1);

            using var image = (Bitmap)cell.GetImage();
            BlitCell(dest, image, cell.TransparentColor, x, y);
        }

        private static void BlitCell(Bitmap dest, Bitmap cell, byte transparent, int destX, int destY)
        {
            var srcArea = new Rectangle(0, 0, cell.Width, cell.Height);
            var src = cell.LockBits(srcArea, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            var dst = dest.LockBits(new Rectangle(0, 0, dest.Width, dest.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                var palette = cell.Palette.Entries;
                var srcStride = src.Stride;
                var srcBytes = new byte[srcStride * cell.Height];
                Marshal.Copy(src.Scan0, srcBytes, 0, srcBytes.Length);

                var dstStride = dst.Stride;
                var dstBytes = new byte[dstStride * dest.Height];
                Marshal.Copy(dst.Scan0, dstBytes, 0, dstBytes.Length);

                for (int y = 0; y < cell.Height; y++)
                {
                    var dy = destY + y;
                    if ((uint)dy >= (uint)dest.Height) continue;
                    var srcRow = y * srcStride;
                    var dstRow = dy * dstStride;
                    for (int x = 0; x < cell.Width; x++)
                    {
                        var index = srcBytes[srcRow + x];
                        if (index == transparent) continue;
                        var dx = destX + x;
                        if ((uint)dx >= (uint)dest.Width) continue;
                        var color = palette[index];
                        var i = dstRow + dx * 4;
                        dstBytes[i] = color.B;
                        dstBytes[i + 1] = color.G;
                        dstBytes[i + 2] = color.R;
                        dstBytes[i + 3] = 255;
                    }
                }

                Marshal.Copy(dstBytes, 0, dst.Scan0, dstBytes.Length);
            }
            finally
            {
                cell.UnlockBits(src);
                dest.UnlockBits(dst);
            }
        }

        private static IEnumerable<ProcedureTree> OrderedProcedures(ScriptAnalyzer analyzer, Object1 room)
        {
            return analyzer.Procedures
                .OrderByDescending(p => p.Name == "init" && p.ClassName == room.Name)
                .ThenByDescending(p => p.Name == "init");
        }

        private static IEnumerable<CallExpr> EnumerateCalls(Expr expr)
        {
            if (expr == null) yield break;
            if (expr is CallExpr call) yield return call;
            if (expr.Args == null) yield break;
            foreach (var arg in expr.Args)
            {
                foreach (var nested in EnumerateCalls(arg))
                    yield return nested;
            }
        }

        private static bool IsRoom(Object1 obj)
        {
            if (!obj.IsInstance) return false;
            for (var current = obj; current != null; current = current.Super)
            {
                if (current.Name == "Rm") return true;
            }
            return false;
        }

        private static byte[] ReadControl(SCIPicture pic)
        {
            if (pic.Vector == null || pic.Vector.Commands.Count == 0) return null;

            var builder = pic.CreateImageBuilder();
            builder.Build();
            var bitmap = builder.Control;
            var width = bitmap.Width;
            var height = bitmap.Height;
            var indices = new byte[width * height];
            var colors = Palette.EGA.Colors;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var argb = bitmap.GetPixel(x, y).ToArgb();
                    byte index = 255;
                    for (byte i = 0; i < colors.Length; i++)
                    {
                        if (colors[i].ToArgb() == argb)
                        {
                            index = i;
                            break;
                        }
                    }
                    indices[y * width + x] = index;
                }
            }

            return indices;
        }

        private static HashSet<Point> ControlRegion(byte[] control, int width, int height, int mask, int x, int y)
        {
            var pixels = new HashSet<Point>();
            for (int bit = 0; bit < 16; bit++)
            {
                if ((mask & (1 << bit)) == 0) continue;
                var start = Nearest(control, width, height, (byte)bit, x, y);
                if (start == null) continue;
                foreach (var point in Region(control, width, height, start.Value))
                    pixels.Add(point);
            }
            return pixels;
        }

        private static Point? Nearest(byte[] control, int width, int height, byte color, int x, int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x >= width) x = width - 1;
            if (y >= height) y = height - 1;
            if (control[y * width + x] == color) return new Point(x, y);

            var best = int.MaxValue;
            Point? found = null;
            for (int iy = 0; iy < height; iy++)
            {
                var row = iy * width;
                var dy = iy - y;
                for (int ix = 0; ix < width; ix++)
                {
                    if (control[row + ix] != color) continue;
                    var dx = ix - x;
                    var dist = dx * dx + dy * dy;
                    if (dist >= best) continue;
                    best = dist;
                    found = new Point(ix, iy);
                }
            }
            return found;
        }

        private static HashSet<Point> Region(byte[] control, int width, int height, Point start)
        {
            var color = control[start.Y * width + start.X];
            var result = new HashSet<Point> { start };
            var stack = new Stack<Point>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var point = stack.Pop();
                Push(point.X - 1, point.Y);
                Push(point.X + 1, point.Y);
                Push(point.X, point.Y - 1);
                Push(point.X, point.Y + 1);
            }

            return result;

            void Push(int px, int py)
            {
                if (px < 0 || py < 0 || px >= width || py >= height) return;
                if (control[py * width + px] != color) return;
                var next = new Point(px, py);
                if (result.Add(next)) stack.Push(next);
            }
        }

        private static Bitmap HighlightRect(Image image, int left, int right, int top, int bottom)
        {
            return DimExcept(image, (x, y) => x >= left && x <= right && y >= top && y <= bottom);
        }

        private static Bitmap HighlightRegion(Image image, HashSet<Point> pixels)
        {
            return DimExcept(image, (x, y) => pixels.Contains(new Point(x, y)));
        }

        private static Bitmap DimExcept(Image source, Func<int, int, bool> keep)
        {
            var bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
                graphics.DrawImage(source, 0, 0, source.Width, source.Height);

            var area = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(area, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                var stride = data.Stride;
                var buffer = new byte[stride * bitmap.Height];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var row = y * stride;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        if (keep(x, y)) continue;
                        var i = row + x * 4;
                        buffer[i] = (byte)(buffer[i] * Dim);
                        buffer[i + 1] = (byte)(buffer[i + 1] * Dim);
                        buffer[i + 2] = (byte)(buffer[i + 2] * Dim);
                    }
                }
                Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        private static string ImagePath(string destFolder, ushort number, string name)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '_');
            return Path.Combine(destFolder, $"{number}.{name}.png");
        }
    }
}
