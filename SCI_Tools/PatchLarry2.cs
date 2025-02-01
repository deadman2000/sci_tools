using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using System;

namespace SCI_Tools
{
    // Русификация Larry 2
    [Command("patch_larry2", Description = "")]
    internal class PatchLarry2 : PatchCommand
    {
        protected override void Patch()
        {
            Patch90();
            Patch103();
            Patch996();
        }

        private void Patch103()
        {
            var res = _translate.GetResource<ResScript>(103);
            var scr = res.GetScript() as Script;

            // Фикс ввода фраз
            var ldi = scr.GetOperator(0x045e);
            if (ldi.Name != "ldi") throw new Exception();
            if (ldi.Arguments[0] is ShortArg) return;

            ldi.Type = 0x34;
            ldi.Arguments[0] = new ShortArg(ldi, 0, 0xff);

            Changed(res);
        }

        private void Patch90()
        {
            var res = _translate.GetResource<ResScript>(90);
            var scr = res.GetScript() as Script;
            SetPushi(scr, 0x439, 86 + 8);
        }

        // Ввод на русском языке
        private void Patch996()
        {
            var res = _translate.GetResource<ResScript>(996);
            var scr = res.GetScript() as Script;
            var ldi = scr.GetOperator(0x142);
            if (ldi.Name != "ldi") throw new Exception();
            if (ldi.Arguments[0] is ShortArg) return;

            ldi.Type = 0x34;
            ldi.Arguments[0] = new ShortArg(ldi, 0, 0xff);
            Changed(res);
        }
    }
}
