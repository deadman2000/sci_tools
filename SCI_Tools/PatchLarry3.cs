using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;

namespace SCI_Tools
{
    // Русификация Larry 3
    [Command("patch_larry3", Description = "")]
    internal class PatchLarry3 : PatchCommand
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
            SetPushi(scr, 0x2bf, 0x80 + 30); // Строчка "Ларри 3:"
            SetPushi(scr, 0x2e8, 0x92 - 30); // Строчка "Страстная Пэтти"
            SetPushi(scr, 0x311, 0x8f - 0); // Строчка "в погоне за"
            SetPushi(scr, 0x33a, 0x84 - 30); // Строчка "грудными мышцами!"
        }
    }
}
