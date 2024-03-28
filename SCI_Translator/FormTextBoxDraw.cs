using SCI_Lib;
using SCI_Lib.Pictures;
using SCI_Lib.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Packaging;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCI_Translator
{
    public partial class FormTextBoxDraw : Form
    {
        private readonly SCIPackage _package;
        private readonly List<ResFont> _fonts;

        public FormTextBoxDraw(SCIPackage package)
        {
            InitializeComponent();

            _package = package;
            _fonts = package.GetResources<ResFont>().ToList();
            cbFont.DataSource = _fonts;
            cbFont.SelectedIndex = 0;
            cbFont.SelectedIndexChanged += OnChanged;

            nudWidth.ValueChanged += OnChanged;
            nudLineHeight.ValueChanged += OnChanged;

            tbText.TextChanged += OnChanged;

            Redraw();
        }

        private void OnChanged(object sender, EventArgs e)
        {
            Redraw();
        }

        void Redraw()
        {
            ResFont res = cbFont.SelectedItem as ResFont;
            int width = (int)nudWidth.Value;
            int spacing = (int)nudLineHeight.Value;
            string text = tbText.Text;

            Draw(res, width, spacing, text);
        }

        private void Draw(ResFont res, int width, int lineHeight, string text)
        {
            var font = res.GetFont();
            var bytes = _package.GameEncoding.GetBytes(text);
            if (lineHeight == 0) lineHeight = font.FontHeight;

            List<Bitmap> lines = new();

            int x = 0;
            int lineWidth;
            List<byte> lineChars = new();
            for (int i = 0; i < bytes.Length; i++)
            {
                var ch = bytes[i];
                if (ch == 0x0d) continue;
                Debug.WriteLine($"{i}: {ch}");
                var symbol = font[ch];
                lineWidth = width == 0 ? x : width;
                if (width > 0 && (x + symbol.Width > width)) // Вылезаем за границу. Надо переносить на новую строку
                {
                    if (ch == 0x20 || ch == 0x0a) // Повезло и мы попали на пробел или перенос строки
                    {
                        lines.Add(GetLine(font, lineWidth, lineHeight, lineChars));

                        lineChars.Clear();
                        x = 0;
                        continue;
                    }
                    else
                    {
                        // Ищем последний пробел
                        var last = lineChars.LastIndexOf(32);
                        if (last == -1) // Пробелов нет. Строка не умещается, рисуем что есть
                        {
                            Debug.WriteLine("Too big line");
                            lines.Add(GetLine(font, lineWidth, lineHeight, lineChars));

                            lineChars.Clear();
                            x = 0;

                            // ...и пропускаем всё до пробела
                            while (i < bytes.Length && bytes[i] != 32) i++;
                            continue;
                        }
                        else
                        {
                            // Добавляем до пробела
                            lines.Add(GetLine(font, lineWidth, lineHeight, lineChars.GetRange(0, last)));

                            // То что справа переносим на следующую строку
                            if (last < lineChars.Count - 1)
                            {
                                lineChars = lineChars.GetRange(last + 1, lineChars.Count - last);
                                x = lineChars.Sum(c => font[c].Width);
                            }
                            else
                            {
                                lineChars.Clear();
                                x = 0;
                            }
                        }
                    }
                }
                else
                {
                    if (ch == 0x0a) // перенос строки
                    {
                        lines.Add(GetLine(font, lineWidth, lineHeight, lineChars));

                        lineChars.Clear();
                        x = 0;
                        continue;
                    }
                }

                x += symbol.Width;
                lineChars.Add(ch);
            }

            if (lineChars.Count > 0)
            {
                lineWidth = width == 0 ? x : width;
                lines.Add(GetLine(font, lineWidth, lineHeight, lineChars));
            }

            if (lines.Count == 0) return;
            if (width == 0)
                width = lines.Max(l => l.Width);

            int scale = 3;
            var totalHeight = lines.Sum(b => b.Height);
            Bitmap bmp = new(width * scale, totalHeight * scale);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                g.FillRegion(new SolidBrush(Color.White), new Region(new Rectangle(Point.Empty, bmp.Size)));
                g.DrawLine(new Pen(Color.Gray, 2), bmp.Width / 2, 0, bmp.Width / 2, bmp.Height);

                int y = 0;
                foreach (var line in lines)
                {
                    g.DrawImage(line, 0, y, line.Width * scale, line.Height * scale);
                    y += line.Height * scale;
                    line.Dispose();
                }
            }
            pbRender.Image?.Dispose();
            pbRender.Image = bmp;
        }

        private static Bitmap GetLine(SCIFont fnt, int width, int height, IEnumerable<byte> text)
        {
            if (width == 0)
                return new Bitmap(1, height);

            var bmp = new Bitmap(width, height);
            int x = 0;
            int y = height - fnt.FontHeight;
            foreach (var ch in text)
            {
                var spr = fnt[ch];
                spr.Draw(bmp, x, y);
                x += spr.Width;
            }
            return bmp;
        }
    }
}
