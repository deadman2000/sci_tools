using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCI_Translator.ImageEditor
{
    class PaintAction : EditAction
    {
        List<PaintActionRecord> _records = new List<PaintActionRecord>();

        public PaintAction(PixelPictureViewer pic)
            : base(pic)
        {
        }

        public override void Undo()
        {
            foreach (PaintActionRecord rec in _records)
            {
                _picture.DrawPixel(rec.Frame, rec.X, rec.Y, rec.OldColor);
            }
        }

        public override void Redo()
        {
            foreach (PaintActionRecord rec in _records)
            {
                _picture.DrawPixel(rec.Frame, rec.X, rec.Y, rec.NewColor);
            }
        }

        public void DrawPixel(int frame, int x, int y, byte oldColor, byte newColor)
        {
            _records.Add(new PaintActionRecord(frame, x, y, oldColor, newColor));
        }

        class PaintActionRecord
        {
            private int _x;
            private int _y;
            private byte _oldColor;
            private byte _newColor;
            private int _frame;

            public PaintActionRecord(int frame, int x, int y, byte oldColor, byte newColor)
            {
                _frame = frame;
                _x = x;
                _y = y;
                _oldColor = oldColor;
                _newColor = newColor;
            }

            public int Frame { get { return _frame; } }

            public int X { get { return _x; } }

            public int Y { get { return _y; } }

            public byte OldColor { get { return _oldColor; } }

            public byte NewColor { get { return _newColor; } }
        }
    }
}
