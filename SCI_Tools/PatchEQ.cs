using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using System;

namespace SCI_Tools
{
    // Русификация EqoQuest
    [Command("patch_eq", Description = "")]
    internal class PatchEQ : PatchCommand
    {
        protected override void Patch()
        {
            Patch0();

            Save();
        }

        private void Patch0()
        {
            var res = _translate.GetResource<ResScript>(0);
            var scr = res.GetScript() as Script;

            var iconRestore = scr.GetInstance("iconRestore");
            if (iconRestore.Properties[7].Value != 64)
            {
                iconRestore.Properties[7].Value = 64;
                Changed(res);
            }

            var iconAbout = scr.GetInstance("iconAbout");
            if (iconAbout.Properties[7].Value != 64)
            {
                iconAbout.Properties[7].Value = 64;
                Changed(res);
            }

            var iconHelp = scr.GetInstance("iconHelp");
            if (iconHelp.Properties[7].Value != 100)
            {
                iconHelp.Properties[7].Value = 100;
                Changed(res);
            }

            var iconQuit = scr.GetInstance("iconQuit");
            if (iconQuit.Properties[7].Value != 136)
            {
                iconQuit.Properties[7].Value = 136;
                Changed(res);
            }
        }
    }
}
