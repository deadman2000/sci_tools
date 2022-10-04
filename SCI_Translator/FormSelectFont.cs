using SCI_Lib.Resources;
using SCI_Translator.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCI_Translator
{
    public partial class FormSelectFont : Form
    {
        public FormSelectFont(ResFont resFont)
        {
            InitializeComponent();

            ilFonts.Images.Clear();
            icbFont.Items.Clear();

            var fonts = resFont.Package.GetResources<ResFont>();
            int i = 0;
            foreach (var f in fonts)
            {
                if (f == resFont) continue;

                var sprite = f.GetFont()['A'];
                var img = sprite.GetImage().Rescale(64);
                ilFonts.Images.Add(img);
                icbFont.Items.Add(new ImageComboItem
                {
                    ImageIndex = i,
                    ItemText = f.FileName,
                    Tag = f,
                });
                i++;
            }
        }

        public ResFont SelectedFont => (ResFont)((ImageComboItem)icbFont.SelectedItem).Tag;

        public int SelectedStartIndex => (int)nudStart.Value;

        public int SelectedEndIndex => (int)nudEnd.Value;

        private void btOK_Click(object sender, EventArgs e)
        {
            if (icbFont.SelectedItem == null) return;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
