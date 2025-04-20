using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;

namespace SCI_Tools
{
    [Command("patch_larry3_pnc", Description = "")]
    internal class PatchLarry3PnC : PatchCommand
    {
        protected override void Patch()
        {
            Patch120();
        }

        private void Patch120()
        {
            // Сдвиг текста
            var res = _translate.GetResource<ResScript>(120);
            var scr = res.GetScript() as Script;
            SetPushi(scr, 0x241, 0x80 + 30); // Строчка "Ларри 3:"
            SetPushi(scr, 0x285, 0x92 - 30); // Строчка "Страстная Пэтти"
            SetPushi(scr, 0x2ae, 0x8f - 0); // Строчка "в погоне за"
            SetPushi(scr, 0x2d6, 0x84 - 30); // Строчка "грудными мышцами!"
        }
    }
}
