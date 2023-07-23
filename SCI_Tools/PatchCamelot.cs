using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources.Vocab;
using System;

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
            RemoveSaidDubl();

            Commit();

            PatchSaid(2, 1, "попроси/*<мерлин"); // попроси у мерлина *

            Save();
        }
    }
}
