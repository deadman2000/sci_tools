using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts1;

namespace SCI_Tools
{
    // Русификация Freddy Pharkas DEMO
    [Command("patch_fp_demo", Description = "")]
    internal class PatchFreddyDemo : PatchCommand
    {
        protected override void Patch()
        {
            Patch15();
            Patch24();
            Patch105();
            Patch150();
        }

        private void Patch15()
        {
            SetHeap(15, 9, "Пустой флакон"); // Empty Vial
            SetHeap(15, 10, "Полный флакон"); // Full Vial
            SetHeap(15, 11, "Бутылка пива"); // Beer Bottle
            SetHeap(15, 12, "Пустая бутылка"); // Empty Bottle
            SetHeap(15, 13, "Полная бутылка"); // Full Bottle
            SetHeap(15, 14, "Уголь"); // Charcoal
            SetHeap(15, 15, "Бутылка из под селитры"); // Saltpeter
            SetHeap(15, 16, "Селитра"); // Full Saltpeter
            SetHeap(15, 17, "Жестяная банка"); // Tin Can
            SetHeap(15, 18, "Пустая кружка"); // Empty Cup
            SetHeap(15, 19, "Полная кружка"); // Full Cup
            SetHeap(15, 20, "Мушка"); // Mole
            SetHeap(15, 21, "Рецепт"); // Prescription
            SetHeap(15, 22, "Фитиль"); // Fuse
            SetHeap(15, 23, "Спички"); // Matches
            SetHeap(15, 24, "Незажжённая бомба"); // Unlit Bomb
            SetHeap(15, 25, "Бомба"); // Bomb
        }

        private void Patch24()
        {
            var res = _translate.GetResource<ResScript>(24);
            var scr = res.GetScript() as Script1;

            // Сдвиг подписей слайдеров
            SetPushi(scr, 0x0230, 0x6e - 4); // Детали
            SetPushi(scr, 0x0241, 0x97 + 2); // Звук
            SetPushi(scr, 0x0253, 0xc3 - 8); // Темп
            SetPushi(scr, 0x0266, 0xe8 - 10); // Текст
        }

        private void Patch105()
        {
            // Кнопки меню на титульном экране
            var res = _translate.GetResource<ResScript>(105);
            var scr = res.GetScript() as Script1;

            SetPushi(scr, 0x027f, 0x9 + 40); // Положение окна по X
            SetPushi(scr, 0x02a0, 0x73 - 25); // X кнопки Баллада
            SetPushi(scr, 0x02b1, 0xe6 - 65); // X кнопки Удрать
        }

        private void Patch150()
        {
            var res = _translate.GetResource<ResScript>(150);
            var scr = res.GetScript() as Script1;

            // Синхронизация шарика с русским текстом
            PatchLocalVars(scr, 36, new ushort[] {
                93, 154, 203, 233,
                78, 141, 195, 220,
                108, 145, 181, 218,
                91, 146, 171, 230,
                77, 139, 194, 235, // Теперь о его жизни слагают истории,
                98, 128, 175, 207,
                63, 97, 130, 156, 194, 235, 272, 272,
                101, 121, 121, 157, 194, 215, 215, 215, // Фаркас, Фредди Фаркас.
                67, 103, 144, 180, 223, 237, 259,
                85, 123, 143, 143, 178, 217, 236, 236, 236, // Фредди Фаркас, Фредди Фаркас.
                109, 130, 144, 144, 204, 204, 204, 204, 204, 204, 204, 204,
                99, 144, 188, 219, // К басне Фредди приступили.
                119, 154, 176, 215,
                58, 100, 131, 131, 181, 223, 258, 258,
                121, 164, 204, 204, // Налетел бандит на город,
                76, 139, 195, 226,
                70, 92, 146, 146, 211, 234, 260, 260,
                101, 121, 121, 157, 194, 215, 215, 215, // Фаркас, Фредди Фаркас.
                77, 109, 149, 185, 215, 231, 245,
                85, 123, 143, 143, 178, 217, 236, 236, 236, // Фредди Фаркас, Фредди Фаркас.
                85, 133, 193, 193, 222, 222, 222, 222, 222, 222, 222, 222,
                124, 158, 207, 227, // Вот она, игра про Фредди
                99, 137, 208, 253,
                54, 87, 128, 148, 174, 222, 240, 240,
                105, 147, 165, 198, // Но в отличие от Ларри
                99, 135, 174, 204,
                61, 61, 120, 120, 177, 209, 247, 247,
                101, 121, 121, 157, 194, 215, 215, 215, // Фаркас, Фредди Фаркас.
                112, 125, 144, 157, 185, 193, 200,
                85, 123, 143, 143, 178, 217, 236, 236, 236, // Фредди Фаркас, Фредди Фаркас.
                100, 158, 158, 190, 233, 233, 233, 233, 233, 233, 233
            });
        }
    }
}
