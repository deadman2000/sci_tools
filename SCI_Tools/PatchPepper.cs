using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts1;

namespace SCI_Tools
{
    // Русификация Pepper's Adventures in Time
    [Command("patch_pepper", Description = "")]
    internal class PatchPepper : PatchCommand
    {
        protected override void Patch()
        {
            Patch110();
        }

        private void Patch110()
        {
            // Кнопки меню на титульном экране
            var res = _translate.GetResource<ResScript>(110);
            var scr = res.GetScript() as Script1;

            SetPushi(scr, 0x1613, 0x1a + 10); // X кнопки АКТ II
            SetPushi(scr, 0x1624, 0x28 + 10); // X кнопки АКТ III
            SetPushi(scr, 0x1635, 0x36 + 10); // X кнопки АКТ IV
            SetPushi(scr, 0x1646, 0x44 + 10); // X кнопки АКТ V
            SetPushi(scr, 0x1657, 0x5b + 10); // X кнопки АКТ VI
        }
    }
}
