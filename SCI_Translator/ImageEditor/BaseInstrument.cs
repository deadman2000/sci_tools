using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace SCI_Translator.ImageEditor
{
    abstract class BaseInstrument
    {
        public static Point PEMPTY = new Point(-1, -1);

        public int Width { get; set; }

        private PixelPictureViewer _pic;

        public PixelPictureViewer Pic
        {
            get { return _pic; }
            set { _pic = value; }
        }

        public abstract string Name { get; }

        public abstract Image Image { get; }

        protected abstract int Order { get; }

        public virtual Cursor Cursor
        {
            get { return null; }
        }

        protected Point _prev = PEMPTY;

        public void OnMouseMove(MouseEventArgs e)
        {
            Point p = Pic.PointToPicture(e.Location);
            if (_prev != p)
            {
                MouseMoved(e.Button, p);
                _prev = p;
            }
        }

        protected virtual void MouseMoved(MouseButtons bt, Point p)
        {
        }

        public virtual void OnMouseDown(MouseEventArgs e)
        {
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
        }

        public void OnMouseLeave()
        {
            MouseLeaved();
            _prev = PEMPTY;
        }

        protected virtual void MouseLeaved()
        {
        }

        public virtual void Draw(Graphics g)
        {
        }

        public virtual void Deactivate()
        {
        }

        protected bool IsNear(Point p1, Point p2)
        {
            return (Math.Abs(p1.X - p2.X) < 2) && (Math.Abs(p1.Y - p2.Y) < 2);
        }

        public static int Compare(BaseInstrument a, BaseInstrument b)
        {
            return a.Order - b.Order;
        }

    }
}
