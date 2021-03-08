using SCI_Lib.Resources.Picture;

namespace SCI_Lib.Resources
{
    public class ResPicture : Resource
    {
        public SCIPicture GetPicture()
        {
            var data = GetContent();
            return new SCIPicture(data);
        }

        public void SetPicture(SCIPicture pic)
        {
            var data = pic.GetBytes();
            SavePatch(data);
        }
    }
}
