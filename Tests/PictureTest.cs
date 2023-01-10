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

            var res = package.GetResource<ResPicture>(100);
            var pic = res.GetPicture() as SCIPicture1;

            var newPic = new SCIPicture1(pic.GetBytes());

            Assert.AreEqual(pic.Image, newPic.Image, "Image encode error");
        }
    }
}
