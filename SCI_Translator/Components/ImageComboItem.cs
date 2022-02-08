using System.Drawing;

namespace SCI_Translator.Components
{
    class ImageComboItem
    {
        public Color ForeColor { get; set; } = Color.Transparent;

        public bool Indicate { get; set; } = false;

        public int ImageIndex { get; set; } = -1;

        public string ItemText { get; set; } = "";

        public object Tag { get; set; }

        public override string ToString() => ItemText;
    }
}
