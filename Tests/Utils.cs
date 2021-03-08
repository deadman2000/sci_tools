using SCI_Lib;

namespace Tests
{
    static class Utils
    {
        public const string ASSETS = "../../../../assets/";

        public const string ConquestPath = ASSETS + "Conquest/";

        public static SCIPackage LoadPackage()
        {
            return SCIPackage.Load(ConquestPath);
        }
    }
}
