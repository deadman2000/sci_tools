using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts1;

namespace SCI_Tools
{
    // Русификация Freddy Pharkas DEMO CD
    [Command("patch_fp_demo_cd", Description = "")]
    internal class PatchFreddyDemoCD : PatchCommand
    {
        protected override void Patch()
        {
            Patch14();
            Patch15();
            Patch24();

            PatchMessages220();
        }

        private void Patch14()
        {
            // Субтитры и речь при запуске игры
            var res = _translate.GetResource<ResScript>(14);
            var scr = res.GetScript() as Script1;

            SetLdi(scr, 0x005a, 3);
        }

        private void Patch15()
        {
            SetHeap(15, 1, "Лек 1"); // Med 1
            SetHeap(15, 2, "Лек 2"); // Med 2
            SetHeap(15, 3, "Лек 3"); // Med 3
            SetHeap(15, 4, "Непр Лек"); // Incorrect Med
            SetHeap(15, 5, "Непр Лек2"); // Incorrect Med2
            SetHeap(15, 6, "Прав рецепт"); // Correct Rx
            SetHeap(15, 18, "Лестница"); // Ladder
            SetHeap(15, 19, "Деньги"); // Money
            SetHeap(15, 20, "Пиво"); // Beer
            SetHeap(15, 21, "Открытое пиво"); // Open Beer
        }

        private void Patch24()
        {
            var res = _translate.GetResource<ResScript>(24);
            var scr = res.GetScript() as Script1;

            // Сдвиг подписей слайдеров
            SetPushi(scr, 0x0255, 109);
            SetPushi(scr, 0x0266, 153);
            SetPushi(scr, 0x0278, 184);
            SetPushi(scr, 0x028b, 219);
        }

        private void PatchMessages220()
        {
            // Фикс порядка сообщений
            var res = _translate.GetResource<ResMessage>(220);
            var messages = res.GetMessages();
            if (messages[45].Seq != 2)
            {
                messages[45].Seq = 2;
                messages[46].Seq = 3;
                Changed(res);
            }
        }
    }
}
