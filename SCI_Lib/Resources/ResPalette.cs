using SCI_Lib.Resources.View;
using SCI_Lib.Utils;
using System.Drawing;
using System.IO;

namespace SCI_Lib.Resources
{
    public class ResPalette : Resource
    {
        public Palette GetPalette()
        {
            var data = GetContent();
            MemoryStream ms = new MemoryStream(data);
            var palette = new Palette();
            palette.Colors = new Color[256];

            ms.Position = 0x25;
            if (data[0x20] == 1)
            {
                int count = (data.Length - 0x25) / 3;
                for (int i = 0; i < count; i++)
                {
                    var r = ms.ReadB();
                    var g = ms.ReadB();
                    var b = ms.ReadB();
                    palette.Colors[i] = Color.FromArgb(r, g, b);
                }
            }
            else
            {
                for (int i = 0; i < 255; i++)
                {
                    var used = ms.ReadB();
                    var r = ms.ReadB();
                    var g = ms.ReadB();
                    var b = ms.ReadB();
                    if (used > 0)
                        palette.Colors[i] = Color.FromArgb(r, g, b);
                }
            }

            return palette;
        }
    }
}
