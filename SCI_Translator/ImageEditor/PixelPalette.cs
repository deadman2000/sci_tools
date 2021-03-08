using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SCI_Translator.ImageEditor
{
    class PixelPalette
    {
        private Color[] colors = null;

        public PixelPalette()
        {
            colors = new Color[256];
        }

        public Color this[byte colorIndex]
        {
            get { return colors[colorIndex]; }
            set { colors[colorIndex] = value; }
        }
    }
}
