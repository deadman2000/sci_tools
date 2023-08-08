using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources.Vocab;

namespace SCI_Tools
{
    // Русификация Conquests of Camelot
    [Command("patch_camelot", Description = "Export font to images")]
    class PatchCamelot : PatchCommand
    {
        protected override void Patch()
        {
            CreateWord("попроси", WordClass.ImperativeVerb);
            CreateWord("у", WordClass.Proposition);

            CreateWord("письменный", WordClass.QualifyingAdjective);
            CreateWord("книжный", WordClass.QualifyingAdjective);

            RemoveSaidDubl();

            Commit();

            PatchSaid(2, 1, "попроси/*<мерлин"); // попроси у мерлина *
            PatchSaid(2, 28, "/митра<герб");
            PatchSaid(2, 29, "//митра<герб");
            PatchSaid(2, 32, "/венера<герб");
            PatchSaid(2, 33, "//венера<герб");
            PatchSaid(2, 40, "/ковер/[!*]");
            PatchSaid(2, 48, "посмотри[<вокруг][/!*][/!*]");
            PatchSaid(2, 56, "/стол[<!*]");
            PatchSaid(2, 57, "//стол[<!*]");
            PatchSaid(2, 58, "/стол<письменный");
            PatchSaid(2, 59, "//стол<письменный");
            PatchSaid(2, 65, "/(полка,шкаф)<книжный,книга");
            PatchSaid(2, 66, "//(полка,шкаф)<книжный,книга");

            Save();
        }
    }
}
