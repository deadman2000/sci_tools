namespace SCI_Translator.ImageEditor
{
    abstract class EditAction
    {
        protected PixelPictureViewer _picture;

        public EditAction(PixelPictureViewer pic)
        {
            _picture = pic;
        }

        public abstract void Undo();

        public abstract void Redo();
    }
}
