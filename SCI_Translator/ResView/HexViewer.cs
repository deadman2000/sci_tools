using System;
using System.Windows.Forms;

namespace SCI_Translator.ResView
{
    class HexViewer : ResViewer
    {
        private readonly TextBox tbHexView;

        public HexViewer()
        {
            tbHexView = new TextBox();
            tbHexView.BackColor = System.Drawing.Color.White;
            tbHexView.Dock = System.Windows.Forms.DockStyle.Fill;
            tbHexView.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            tbHexView.Multiline = true;
            tbHexView.ReadOnly = true;
            tbHexView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            tbHexView.WordWrap = false;

            Controls.Add(this.tbHexView);
        }

        protected override void Reload()
        {
            try
            {
                byte[] data = Current.GetContent();

                if (data == null)
                {
                    tbHexView.Text = "Unsupported compression method";
                    return;
                }

                tbHexView.Text = GameEncoding.ByteToHexTable(data);
            }
            catch (Exception ex)
            {
                tbHexView.Text = ex.ToString();
            }
        }
    }
}
