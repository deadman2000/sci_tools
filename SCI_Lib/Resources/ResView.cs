using SCI_Lib.Resources.View;

namespace SCI_Lib.Resources
{
    public class ResView : Resource
    {
        public SCIView GetView()
        {
            var data = GetContent();
            return new SCIView(data);
        }
    }
}
