using NUnit.Framework;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Picture;

namespace Tests
{
    public class PictureTest
    {
        [Test]
        public void ImagePackTest()
        {
            var package = Utils.LoadPackage();

            var res = package.GetResouce<ResPicture>(100);
            var pic = res.GetPicture();

            var newPic = new SCIPicture(pic.GetBytes());

            Assert.AreEqual(pic.Image, newPic.Image, "Image encode error");
        }
    }
}
