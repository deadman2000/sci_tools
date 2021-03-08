using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SCI_Translator.ImageEditor
{
    public partial class NewSizeDialog : Form
    {
        public NewSizeDialog(int w, int h)
        {
            InitializeComponent();

            nudWidth.Value = w;
            nudHeight.Value = h;
            SelectAll(nudWidth);
            SelectAll(nudHeight);
            nudWidth.Focus();
        }

        private void SelectAll(NumericUpDown nud)
        {
            nud.Select(0, nud.Text.Length);
        }

        public int SWidth { get { return (int)nudWidth.Value; } }

        public int SHeight { get { return (int)nudHeight.Value; } }

        private void btOK_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

    }
}
