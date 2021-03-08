using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using SCI_Lib.Pictures;

namespace SCI_Translator.ImageEditor
{
    partial class PixelPictureViewer : Control
    {
        private Image _backGround = null;

        // Privates
        private Sprite _sprite;
        private int _currentFrame;

        private PixelPalette _palette;
        private Bitmap _bmp;
        private Bitmap _top;
        private Graphics _gTop;

        private int _zoom = 1;
        private bool _drawGrid = true;
        private Pen gridPen;

        // Constructors

        public PixelPictureViewer()
        {
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint, true);

            gridPen = new Pen(Color.Black);
            gridPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
        }

        public PixelPictureViewer(IContainer container)
            : this()
        {
            container.Add(this);
        }

        // Properties

        [Browsable(false)]
        [DefaultValue(null)]
        public Sprite Sprite
        {
            get { return _sprite; }
            set
            {
                _sprite = value;
                _currentFrame = 0;
                ClearUndoRedo();
                CalcComponentSize();
                Redraw();
            }
        }

        [Browsable(false)]
        [DefaultValue(null)]
        public PixelPalette Palette
        {
            get { return _palette; }
            set
            {
                _palette = value;
                Redraw();
            }
        }

        [Browsable(false)]
        [DefaultValue(null)]
        public Image Background
        {
            get { return _backGround; }
            set { _backGround = value; }
        }


        [Browsable(false)]
        [DefaultValue(1)]
        public int Zoom
        {
            get { return _zoom; }
            set
            {
                if (_zoom != value)
                {
                    _zoom = value;
                    CalcComponentSize();
                    Redraw();
                }
            }
        }

        [DefaultValue(true)]
        public bool DrawGrid
        {
            get { return _drawGrid; }
            set
            {
                _drawGrid = value;
                Redraw();
            }
        }

        [Browsable(false)]
        public SpriteFrame CurrentFrame
        {
            get {
                if (_currentFrame >= _sprite.Frames.Count) return null;
                return _sprite[_currentFrame];
            }
        }

        public int CurrentFrameIndex
        {
            get { return _currentFrame; }
            set
            {
                _currentFrame = value;
                CalcComponentSize();
            }
        }

        public Graphics GTop
        {
            get { return _gTop; }
        }

        public BaseInstrument CurrInstrument
        {
            get;
            set;
        }

        // Methods

        public void Redraw()
        {
            if (_sprite == null || _sprite.Frames.Count == 0) return;

            if (Width != CurrentFrame.Width * _zoom ||Height != CurrentFrame.Height * _zoom)
                CalcComponentSize();

            _bmp = CreateBitmap(_currentFrame);
            if (_sprite != null)
            {
                _top = new Bitmap(CurrentFrame.Width * _zoom, CurrentFrame.Height * _zoom);
                _gTop = Graphics.FromImage(_top);
                _gTop.RenderingOrigin = new Point(-2, -2);
                _gTop.CompositingMode = CompositingMode.SourceCopy;
                _gTop.CompositingQuality = CompositingQuality.HighSpeed;
                _gTop.InterpolationMode = InterpolationMode.NearestNeighbor;
            }

            this.Invalidate();
        }

        public void CalcComponentSize()
        {
            if (_sprite == null || _sprite.Frames.Count == 0) return;
            this.Width = CurrentFrame.Width * _zoom;
            this.Height = CurrentFrame.Height * _zoom;
        }

        public Bitmap CreateBitmap(int frame)
        {
            if (_sprite == null || _palette == null) return null;

            Bitmap bmp = new Bitmap(_sprite[frame].Width, _sprite[frame].Height);

            for (int x = 0; x < _sprite[frame].Width; x++)
                for (int y = 0; y < _sprite[frame].Height; y++)
                    bmp.SetPixel(x, y, _palette[_sprite[frame][x, y]]);

            return bmp;
        }

        private HatchBrush _bTransparent = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.LightGray, Color.White);

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_sprite == null || _bmp == null)
            {
                base.OnPaint(e);
                return;
            }

            e.Graphics.SetClip(e.ClipRectangle, System.Drawing.Drawing2D.CombineMode.Replace);
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(_bmp, 0, 0, CurrentFrame.Width * _zoom, CurrentFrame.Height * _zoom);

            if (_drawGrid && _zoom >= 5)
            {
                for (int x = 0; x < CurrentFrame.Width; x++)
                    e.Graphics.DrawLine(gridPen, x * _zoom, 0, x * _zoom, CurrentFrame.Height * _zoom);

                for (int y = 0; y < CurrentFrame.Height; y++)
                    e.Graphics.DrawLine(gridPen, 0, y * _zoom, CurrentFrame.Width * _zoom, y * _zoom);
            }

            if (CurrInstrument != null)
                CurrInstrument.Draw(_gTop);
            e.Graphics.DrawImage(_top, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
        }

        public Point PointToPicture(Point p)
        {
            return new Point(p.X / _zoom, p.Y / _zoom);
        }

        public void InvalidateSquare(Point p, int w)
        {
            InvalidateSquare(p.X, p.Y, w);
        }

        public void InvalidateSquare(int x, int y, int w)
        {
            this.Invalidate(new Rectangle(x * _zoom - w / 2 * _zoom, y * _zoom - w / 2 * _zoom, w * _zoom, w * _zoom));
        }

        private void InvalidatePixel(int x, int y)
        {
            this.Invalidate(new Rectangle(x * _zoom, y * _zoom, _zoom, _zoom));
        }

        #region Редактирование

        List<EditAction> _undoStack = new List<EditAction>();
        List<EditAction> _redoStack = new List<EditAction>();
        EditAction _currentAction = null;
        bool _actionsRecord = true;

        public void ClearUndoRedo()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        public void DrawPixel(int frame, Point p, byte color)
        {
            DrawPixel(frame, p.X, p.Y, color);
        }

        public void DrawPixel(int frame, int x, int y, byte color)
        {
            if (x < 0 || x >= _sprite[frame].Width || y < 0 || y >= _sprite[frame].Height) return;
            if (_sprite[frame][x, y] == color) return;

            if (_actionsRecord)
            {
                if (!(_currentAction is PaintAction))
                {
                    CommitEdit();
                    _currentAction = new PaintAction(this);
                }

                PaintAction action = (PaintAction)_currentAction;
                action.DrawPixel(frame, x, y, _sprite[frame][x, y], color);
            }

            _sprite[frame][x, y] = color;
            if (frame == _currentFrame)
            {
                _bmp.SetPixel(x, y, _palette[color]);
                InvalidatePixel(x, y);
            }
        }

        public void DrawSquare(Point p, byte color, int width)
        {
            DrawSquare(p.X, p.Y, color, width);
        }

        public void DrawSquare(int x, int y, byte color, int width)
        {
            for (int dX = 0; dX < width; dX++)
                for (int dY = 0; dY < width; dY++)
                {
                    DrawPixel(_currentFrame, x + dX - width / 2, y + dY - width / 2, color);
                }
        }

        public static Point[] CreateLine(Point p1, Point p2)
        {
            List<Point> points = new List<Point>();

            Point min = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            Point max = new Point(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));

            double y;
            double x;
            double dX;
            double dY;

            if (max.X - min.X >= max.Y - min.Y)
            {
                dX = 1;
                if (max.Y == min.Y)
                    dY = 0;
                else
                    dY = (double)(max.Y - min.Y) / (max.X - min.X);
            }
            else
            {
                dY = 1;
                if (max.X == min.X)
                    dX = 0;
                else
                    dX = (double)(max.X - min.X) / (max.Y - min.Y);
            }

            y = min.Y;
            if ((p1.X > p2.X && p1.Y > p2.Y) || (p2.X > p1.X && p2.Y > p1.Y))
            {
                x = min.X;
            }
            else
            {
                x = max.X;
                dX = -dX;
            }

            //Console.WriteLine("B {0} {1} - {2} {3}", p1.X, p1.Y, p2.X, p2.Y);
            int ix, iy;
            while (true)
            {
                ix = (int)Math.Round(x);
                iy = (int)Math.Round(y);

                if ((dX != 0 && (ix > max.X || ix < min.X)) || (dY != 0 && iy > max.Y))
                {
                    //Console.WriteLine("E {0} {1} - {2} {3}", p1.X, p1.Y, ix, iy);
                    break;
                }

                points.Add(new Point(ix, iy));
                //Console.WriteLine("[{0}] {1} {2} ({3} {4})", points.Count - 1, ix, iy, x, y);
                x = dX + x;
                y = dY + y;
            }

            return points.ToArray();
        }

        public void DrawSquareLine(Point p1, Point p2, byte color, int width)
        {
            Point[] line = CreateLine(p1, p2);
            foreach (Point p in line)
            {
                DrawSquare((int)p.X, (int)p.Y, color, width);
            }
        }

        public static int UndoStackLength = 100;

        public void CommitEdit()
        {
            if (_currentAction != null)
            {
                _undoStack.Add(_currentAction);
                while (_undoStack.Count > UndoStackLength)
                    _undoStack.RemoveAt(0);
                _redoStack.Clear();
                _currentAction = null;
            }
        }

        public void RollbackEdit()
        {
            _actionsRecord = false;
            _currentAction.Undo();
            _currentAction = null;
            _actionsRecord = true;
        }

        public void Undo()
        {
            if (_currentAction != null)
                _currentAction.Undo();
            else if (_undoStack.Count > 0)
            {
                EditAction action = _undoStack[_undoStack.Count - 1];

                _actionsRecord = false;
                action.Undo();
                _actionsRecord = true;

                _undoStack.Remove(action);
                _redoStack.Add(action);
            }
        }

        public bool HasUndo
        {
            get { return _undoStack.Count > 0; }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                EditAction action = _redoStack[_redoStack.Count - 1];

                _actionsRecord = false;
                action.Redo();
                _actionsRecord = true;

                _redoStack.Remove(action);
                _undoStack.Add(action);
            }
        }

        public bool HasRedo
        {
            get { return _redoStack.Count > 0; }
        }

        #endregion

        #region Операции для слоев

        SolidBrush _bAdd = new SolidBrush(Color.Black);

        public void LClearSquare(Point p, int w)
        {
            LClearSquare(p.X, p.Y, w);
        }

        public void LClearSquare(int x, int y, int w)
        {
            int x1 = x * _zoom;
            int y1 = y * _zoom;
            _bAdd.Color = Color.Transparent;
            _gTop.FillRectangle(_bAdd, x1 - w / 2 * _zoom, y1 - w / 2 * _zoom, w * _zoom, w * _zoom);
            InvalidateSquare(x, y, w);
        }

        public void LDrawSquare(Point p, byte color, int w)
        {
            LDrawSquare(p.X, p.Y, color, w);
        }

        public void LDrawSquare(int x, int y, byte color, int w)
        {
            int x1 = x * _zoom;
            int y1 = y * _zoom;

            _bAdd.Color = _palette[color];
            _gTop.FillRectangle(_bAdd, x1 - w / 2 * _zoom, y1 - w / 2 * _zoom, w * _zoom, w * _zoom);

            InvalidateSquare(x, y, w);
        }

        public void LDrawSquareLine(Point p1, Point p2, byte color, int width)
        {
            Point[] line = CreateLine(p1, p2);
            foreach (Point p in line)
            {
                LDrawSquare((int)p.X, (int)p.Y, color, width);
            }
        }

        #endregion

        public void InvalidateLine(Point p1, Point p2)
        {
            Point[] line = CreateLine(p1, p2);
            foreach (Point p in line)
            {
                InvalidateSquare((int)p.X, (int)p.Y, Width);
            }
        }
    }
}
