using SCI_Lib;

namespace Tests
{
    static class Utils
    {
        public const string ASSETS = "../../../../assets/";

        public const string ConquestPath = ASSETS + "Conquest/";
        public const string EQPath = ASSETS + "EQ/";

        public static SCIPackage LoadConquest()
        {
            return SCIPackage.Load(ConquestPath);
        }

        public static SCIPackage LoadEQ()
        {
            return SCIPackage.Load(EQPath);
        }
    }
}
