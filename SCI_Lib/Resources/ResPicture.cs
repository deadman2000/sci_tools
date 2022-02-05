using SCI_Lib.Resources.Picture;

namespace SCI_Lib.Resources
{
    public class ResPicture : Resource
    {
        private SCIPicture _pic;

        public SCIPicture GetPicture()
        {
            if (_pic != null) return _pic;
            var data = GetContent();
            return _pic = new SCIPicture(data);
        }

        public void SetPicture(SCIPicture pic)
        {
            _pic = pic;
        }

        public override byte[] GetPatch()
        {
            return _pic.GetBytes();
        }
    }
}
