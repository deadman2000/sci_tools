using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
