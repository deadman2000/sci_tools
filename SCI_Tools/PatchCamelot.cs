using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using System;

namespace SCI_Tools
{
    // Русификация Conquests of Camelot
    [Command("patch_camelot", Description = "Export font to images")]
    class PatchCamelot : PatchCommand
    {
        protected override void Patch()
        {
            PatchScript3();
            Save();
        }

        private void PatchScript3()
        {
            var res = _translate.GetResource<ResScript>(3);
            var scr = res.GetScript() as Script;
            var op = scr.GetOperator(0x1b2);
            if (op.Name != "pushi") throw new Exception();

            if ((ushort)op.Arguments[0] == 165) return;
            op.Arguments[0] = (ushort)165;
            Changed(res);
        }
    }
}
