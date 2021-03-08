using SCI_Lib.Resources;

namespace SCI_Translator.ResView
{
    partial class PicView : ResViewer
    {
        public PicView()
        {
            InitializeComponent();
        }

        protected override void Reload()
        {
            try
            {
                var resPic = (ResPicture)Current;
                var pic = resPic.GetPicture();
                var img = pic.GetBackground();
                if (img != null)
                {
                    pictureBox1.Width = img.Width;
                    pictureBox1.Height = img.Height;
                    pictureBox1.Image = img;
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
            catch
            {
                pictureBox1.Image = null;
            }
        }
    }
}
