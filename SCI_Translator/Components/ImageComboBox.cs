using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCI_Translator.Components
{
    class ImageComboBox : ComboBox
    {
        public ImageComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
        }

        public ImageList Images { get; set; }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();
            var w = Math.Max(e.Bounds.Height, Images.ImageSize.Width);

            if (e.Index < 0)
                e.Graphics.DrawString(Text, e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + w, e.Bounds.Top);
            else
            {
                if (Items[e.Index].GetType() == typeof(ImageComboItem))
                {
                    ImageComboItem icItem = (ImageComboItem)Items[e.Index];
                    Color forecolor = (icItem.ForeColor != Color.FromKnownColor(KnownColor.Transparent)) ? icItem.ForeColor : e.ForeColor;
                    Font font = icItem.Indicate ? new Font(e.Font, FontStyle.Bold) : e.Font;

                    if (icItem.ImageIndex != -1)
                    {
                        Images.Draw(e.Graphics, e.Bounds.Left, e.Bounds.Top, w, e.Bounds.Height, icItem.ImageIndex);
                        e.Graphics.DrawString(icItem.ItemText, font, new SolidBrush(forecolor), e.Bounds.Left + w, e.Bounds.Top);
                    }
                    else
                        e.Graphics.DrawString(icItem.ItemText, font, new SolidBrush(forecolor), e.Bounds.Left + w, e.Bounds.Top);
                }
                else
                    e.Graphics.DrawString(Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + w, e.Bounds.Top);
            }

            base.OnDrawItem(e);
        }
    }
}
