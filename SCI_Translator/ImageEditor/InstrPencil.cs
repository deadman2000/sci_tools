using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace SCI_Translator.ImageEditor
{
    class InstrPencil : BaseInstrument
    {
        public InstrPencil()
        {
        }

        public InstrPencil(int width, byte colorA, byte colorB)
        {
            Width = width;
            _colorA = colorA;
            _colorB = colorB;
        }

        public override string Name { get { return "Карандаш"; } }

        public override Image Image { get { return Properties.Resources.pencil; } }

        protected override int Order { get { return 1; } }

        //static Cursor cursor = NativeMethods.LoadCustomCursorSafe(Application.StartupPath + "\\cur\\pencil.cur", Cursors.Default);
        //public override Cursor Cursor { get { return cursor; } }

        public override void OnMouseDown(MouseEventArgs e)
        {
            MouseMoved(e.Button, Pic.PointToPicture(e.Location));
        }

        private byte _colorA;
        private byte _colorB;

        public byte ColorA
        {
            get { return _colorA; }
            set { _colorA = value; }
        }

        public byte ColorB
        {
            get { return _colorB; }
            set { _colorB = value; }
        }

        protected override void MouseMoved(MouseButtons bt, Point p)
        {
            if (_prev != PEMPTY)
                Pic.LClearSquare(_prev, Width);

            byte c;
            if (bt == System.Windows.Forms.MouseButtons.Left)
                c = _colorA;
            else if (bt == System.Windows.Forms.MouseButtons.Right)
                c = _colorB;
            else
            {
                Pic.LDrawSquare(p, _colorA, Width);
                return;
            }

            if (_prev == PEMPTY || IsNear(_prev, p))
                Pic.DrawSquare(p.X, p.Y, c, Width);
            else
                Pic.DrawSquareLine(_prev, p, c, Width);
        }

        protected override void MouseLeaved()
        {
            if (_prev != PEMPTY)
            {
                Pic.LClearSquare(_prev, Width);
                _prev = PEMPTY;
            }
        }
    }
}
