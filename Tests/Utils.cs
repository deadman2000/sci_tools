using SCI_Lib;

namespace Tests
{
    static class Utils
    {
        public const string ASSETS = "../../../../assets/";

        public static SCIPackage LoadConquest()
        {
            return SCIPackage.Load(ASSETS + "Conquest/");
        }

        public static SCIPackage LoadEQ()
        {
            return SCIPackage.Load(ASSETS + "EQ/");
        }

        public static SCIPackage LoadQG()
        {
            return SCIPackage.Load(ASSETS + "QG_VGA/");
        }
    }
}
