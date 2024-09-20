using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using System;

namespace SCI_Tools
{
    // Русификация Larry 5
    [Command("patch_larry5", Description = "")]
    internal class PatchLarry5 : PatchCommand
    {
        protected override void Patch()
        {
            Patch0();
            Patch100();
            Patch120();
            Patch170();
            Patch205();
            Patch415();
            Patch720();
        }

        private void Patch0()
        {
            var res = _translate.GetResource<ResScript>(0);
            var scr = res.GetScript();

            SetPushi(scr, 0x20d5, 0xba - 1);
        }

        private void Patch100()
        {
            var res = _translate.GetResource<ResScript>(100);
            var scr = res.GetScript();

            // Стираемый текст
            const byte delete_beg = 23; // 29(1d)
            const byte delete_end = 19; // 22(16)
            const short letterX = 128; // 153(99)

            SetLdi(scr, 0x401, delete_beg);
            SetLdi(scr, 0x42f, delete_end);
            SetLdi(scr, 0x441, delete_end);
            SetLdi(scr, 0x450, letterX);
            SetLdi(scr, 0x455, delete_beg);


            SetPushi(scr, 0xa82, delete_end - 1);
            SetLdi(scr, 0xa8b, delete_beg);
            SetLdi(scr, 0xa95, delete_end);
        }

        private void Patch120()
        {
            var res = _translate.GetResource<ResScript>(120);
            var scr = res.GetScript();

            // Делаем шире окно текста и смещаем левее
            var bruno = scr.GetInstance("Bruno", "Talker");
            bruno ??= scr.GetInstance("Бруно", "Talker");

            const int shift = 90;
            const int brunoX = 115 - shift;
            const int brunoW = 200 + shift;
            if (bruno.GetProperty("x") != brunoX)
            {
                bruno.SetProperty("x", brunoX);
                Changed(res);
            }
            if (bruno.GetProperty("talkWidth") != brunoW)
            {
                bruno.SetProperty("talkWidth", brunoW);
                Changed(res);
            }
        }

        private void Patch170()
        {
            var res = _translate.GetResource<ResScript>(170);
            var scr = res.GetScript();

            // Сдвиг кнопок влево чтобы выровнять по центру
            SetPushi(scr, 0x114a, 0x46 - 20);
            SetPushi(scr, 0x115b, 0x5f - 5);
            SetPushi(scr, 0x11f2, 0x46 - 5);
            SetPushi(scr, 0x1203, 0x5f + 18);
        }

        private void Patch205()
        {
            var res = _translate.GetResource<ResScript>(205);
            var scr = res.GetScript();

            var bruno = scr.GetInstance("Bruno", "Talker");
            bruno ??= scr.GetInstance("Бруно", "Talker");

            const int shift = 40;
            const int brunoX = 100 - shift;
            SetProperty(scr, bruno, "x", brunoX);
        }

        private void Patch415()
        {
            var res = _translate.GetResource<ResScript>(415);
            var scr = res.GetScript();

            var desmond = scr.GetInstance("Инспектор Десмонд", "Talker");
            desmond ??= scr.GetInstance("Inspector Desmond", "Talker");

            SetProperty(scr, desmond, "talkWidth", 160 + 20);
        }

        private void Patch720()
        {
            var res = _translate.GetResource<ResScript>(720);
            var scr = res.GetScript();

            // Позиции верхних подписей
            SetPushi(scr, 0x065f, 0x50 + 27); // Выигрыш
            SetPushi(scr, 0x069a, 0x99 + 21); // Ставка
            SetPushi(scr, 0x06d6, 0xf2 - 2);  // Баланс

            // Позиции кнопок
            SetProperty(scr, scr.GetInstance("hold1"), "x", 68 + 5);
            SetProperty(scr, scr.GetInstance("hold2"), "x", 114 + 3);
            SetProperty(scr, scr.GetInstance("hold3"), "x", 159 + 1);
            SetProperty(scr, scr.GetInstance("cashout"), "x", 287 + 2);
        }

    }
}
