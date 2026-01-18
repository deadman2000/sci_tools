using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts1;
using System;

namespace SCI_Tools
{
    // Русификация Freddy Pharkas CD
    [Command("patch_fp", Description = "")]
    internal class PatchFreddy : PatchCommand
    {
        protected override void Patch()
        {
            PatchMessages640();
            PatchMessages1039();

            Patch15();
            Patch24();
            Patch25();
            Patch90();
            Patch110();
            Patch150();
            Patch170();
            Patch625();
            Patch740();
            Patch790();
        }

        private void PatchMessages640()
        {
            // Фикс реплики Сэла
            var res = _translate.GetResource<ResMessage>(640);
            var messages = res.GetMessages();
            if (messages[33].Talker != 43)
            {
                messages[33].Talker = 43;
                Changed(res);
            }
        }

        private void PatchMessages1039()
        {
            var res = _translate.GetResource<ResMessage>(1039);
            var messages = res.GetMessages();
            if (messages[1].Verb != 68)
            {
                messages[1].Verb = 68;
                messages[1].Seq = 1;
                Changed(res);
            }
        }

        private void Patch15()
        {
            SetHeap(15, 1, "Лек 1"); // Med 1
            SetHeap(15, 2, "Лек 2"); // Med 2
            SetHeap(15, 3, "Лек 3"); // Med 3
            SetHeap(15, 4, "Непр Лек"); // Incorrect Med
            SetHeap(15, 5, "Непр Лек2"); // Incorrect Med2
            SetHeap(15, 6, "Прав рецепт"); // Correct Rx
            SetHeap(15, 18, "Ключ от двери"); // Door Key
            SetHeap(15, 19, "Рецепт Пенелопы"); // Penelope's Rx
            SetHeap(15, 20, "Лекарство"); // Medication
            SetHeap(15, 21, "Рецепт Хелен"); // Helen's Rx
            SetHeap(15, 22, "Рецепт Мадам"); // Madame's Rx
            SetHeap(15, 23, "Стакан"); // Shot Glass
            SetHeap(15, 24, "Под стаканом"); // Under Glass
            SetHeap(15, 25, "Препарат Г"); // Prep G
            SetHeap(15, 26, "H2O из башни"); // Tower H20
            SetHeap(15, 27, "Консервная банка"); // Tin Can
            SetHeap(15, 28, "Ледокол"); // Ice Pick
            SetHeap(15, 29, "Уголь"); // Charcoal
            SetHeap(15, 30, "Ремень"); // Leather Strap
            SetHeap(15, 31, "Противогаз"); // Gas Mask
            SetHeap(15, 32, "Дефлатулянт"); // Deflatulizer
            SetHeap(15, 33, "Улитки"); // Snails
            SetHeap(15, 34, "Деньги"); // Money
            SetHeap(15, 35, "Пиво"); // Beer
            SetHeap(15, 36, "Открытое пиво"); // Open Beer
            SetHeap(15, 37, "Бутылки"); // Empty Bottles
            SetHeap(15, 38, "Ключ от церкви"); // Church Key
            SetHeap(15, 39, "Лестница"); // Ladder
            SetHeap(15, 40, "Верёвка"); // Rope
            SetHeap(15, 41, "Лассо"); // Lasso
            SetHeap(15, 42, "Очиститель"); // Pure Solution
            SetHeap(15, 43, "Оксид азота"); // Nitrous Oxide
            SetHeap(15, 44, "Сода"); // Baking Soda
            SetHeap(15, 45, "Открытки"); // Post Cards
            SetHeap(15, 46, "Воск"); // Candle Wax
            SetHeap(15, 47, "Нож"); // Knife
            SetHeap(15, 48, "Ключ от стола"); // Desk Key
            SetHeap(15, 49, "Письмо"); // Letter
            SetHeap(15, 50, "Лопата"); // Shovel
            SetHeap(15, 51, "Ключ от ячейки"); // Deposit Key
            SetHeap(15, 52, "Пистолеты"); // Pistols
            SetHeap(15, 53, "Пирог"); // Pie
            SetHeap(15, 54, "Кофе"); // Coffee
            SetHeap(15, 55, "Набор для чистки"); // Cleaning Kit
            SetHeap(15, 56, "Патроны"); // Bullets
            SetHeap(15, 57, "Глина"); // Clay
            SetHeap(15, 58, "Медаль"); // Medallion
            SetHeap(15, 59, "Пустая форма"); // Empty Mold
            SetHeap(15, 60, "Форма с воском"); // Wax Filled Mold
            SetHeap(15, 61, "Форма с серебром"); // Silver Filled Mold
            SetHeap(15, 62, "Ухо из воска"); // Wax Ear
            SetHeap(15, 63, "Ухо из серебра"); // Silver Ear
            SetHeap(15, 64, "Одежда"); // Clothes
            SetHeap(15, 65, "Квитанция"); // Claim Check
            SetHeap(15, 66, "Сапоги"); // Boots
            SetHeap(15, 67, "Бандана"); // Neckerchief
            SetHeap(15, 68, "Заточенное ухо"); // Sharp Ear
            SetHeap(15, 69, "Меч"); // Sword
            SetHeap(15, 70, "Бумажный пакет"); // Paper Sack
            SetHeap(15, 71, "Полный пакет"); // Filled Sack
            SetHeap(15, 72, "Эликсир"); // Elixir
            SetHeap(15, 73, "Лепёшка"); // Horse Plop
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

        private void Patch25()
        {
            SetHeap(25, 1, "Назад"); // Outta here!
        }

        private void Patch90()
        {
            SetHeap(90, 0, "Проказник");
            SetHeap(90, 1, "Кэрри Сью");
            SetHeap(90, 2, "Бродяга, городской пёс");
            SetHeap(90, 3, "Счастливчик");
            SetHeap(90, 4, "Трикси, городская овца");
            SetHeap(90, 5, "Утка дока Гиллеспи");
            SetHeap(90, 6, "Полковник Шандерс");
            SetHeap(90, 7, "Фрэнсис Бэкон");
            SetHeap(90, 8, "See them tumblin' down");
            SetHeap(90, 9, "Сын проповедника");
            SetHeap(90, 10, "Оборванка");
            SetHeap(90, 11, "Старина Пит Кирка");
            SetHeap(90, 12, "Офицер Уилки");
            SetHeap(90, 13, "Курьер Пони Экспресс");
            SetHeap(90, 14, "Клем");
            SetHeap(90, 15, "Амос");
            SetHeap(90, 16, "Мисс Такер");
            SetHeap(90, 17, "Мисс Евлалия");
            SetHeap(90, 18, "Бриджет и Рокко О'Ханахан");
        }

        private void Patch110()
        {
            // Кнопки меню на титульном экране
            var res = _translate.GetResource<ResScript>(110);
            var scr = res.GetScript() as Script1;

            SetPushi(scr, 0x02cc, 0x2e - 22); // Положение окна по X
            SetPushi(scr, 0x02eb, 0x37 + 10); // X кнопки Пролог
            SetPushi(scr, 0x02fc, 0x6e + 4); // X кнопки Играть
            SetPushi(scr, 0x030d, 0x95 + 12); // X кнопки Помощь
            SetPushi(scr, 0x031e, 0xbc + 22); // X кнопки Выход
        }

        private void Patch150()
        {
            var res = _translate.GetResource<ResScript>(150);
            var scr = res.GetScript() as Script1;

            // Синхронизация шарика с русским текстом
            PatchLocalVars(scr, 38, new ushort[] {
                69, 106, 158, 195, 233,
                59, 127, 186, 240,
                42, 55, 112, 145, 170, 210, 254, 278,
                105, 158, 196, 232,
                71, 115, 175, 248,
                51, 68, 108, 150, 175, 214, 272, 272,
                102, 123, 123, 157, 195, 215, 215, 215,
                85, 103, 125, 159, 183, 201, 222,
                86, 123, 145, 145, 180, 218, 237, 237, 237,
                98, 125, 160, 190, 222, 222, 222, 222, 222, 222, 222, 222,
                70, 148, 178, 225,
                60, 110, 215, 265,
                65, 82, 123, 165, 191, 219, 245, 245,
                62, 114, 191, 254,
                44, 126, 212, 257,
                61, 107, 136, 136, 172, 202, 236, 261,
                49, 113, 165, 216,
                65, 161, 202, 251,
                59, 78, 138, 162, 175, 192, 236, 261,
                76, 119, 208, 257,
                59, 101, 162, 220,
                97, 110, 170, 210, 236, 236, 236, 236,
                102, 123, 123, 157, 195, 215, 215, 215,
                88, 112, 140, 168, 219, 245, 245,
                86, 123, 145, 145, 180, 218, 237, 237, 237,
                70, 122, 156, 208, 250, 250, 250, 250, 250, 250, 250, 250,
                64, 148, 212, 241,
                82, 120, 201, 262,
                64, 106, 141, 152, 176, 215, 269, 269,
                68, 126, 172, 235,
                49, 100, 172, 265,
                55, 118, 162, 226, 250, 268, 268, 268,
                85, 152, 195, 245,
                96, 152, 192, 249,
                79, 116, 144, 182, 212, 239, 252, 252,
                65, 115, 197, 259,
                87, 155, 189, 235,
                52, 108, 151, 196, 238, 270, 282, 282,
                102, 123, 123, 157, 195, 215, 215, 215,
                68, 102, 166, 202, 214, 226, 248,
                86, 123, 145, 145, 180, 218, 237, 237, 237,
                70, 113, 126, 166, 180, 224, 245, 245, 245, 245
            });

            // Кнопки меню в прологе
            SetPushi(scr, 0x051f, 0x2e - 29); // Положение окна по X
            SetPushi(scr, 0x0542, 0x37 + 10); // X кнопки Продолжить
            SetPushi(scr, 0x0553, 0x6e + 19); // X кнопки Играть
            SetPushi(scr, 0x0564, 0x95 + 27); // X кнопки Помощь
            SetPushi(scr, 0x0575, 0xbc + 37); // X кнопки Выход
        }

        private void Patch170()
        {
            var res = _translate.GetResource<ResScript>(170);
            var scr = res.GetScript() as Script1;

            // Синхронизация шарика с русским текстом
            PatchLocalVars(scr, 38, new ushort[] {
                79, 106, 176, 200, 229,
                60, 130, 180, 283,
                46, 114, 172, 212, 245, 272, 272, 272,
                69, 122, 189, 252,
                71, 144, 196, 271,
                31, 55, 116, 186, 216, 251, 286, 286,
                86, 134, 169, 205,
                58, 104, 179, 260,
                75, 126, 170, 200, 254, 254, 254, 254,
                86, 115, 180, 220,
                75, 132, 197, 231,
                61, 80, 128, 154, 192, 210, 249, 249,
                102, 123, 123, 157, 195, 215, 215, 215,
                50, 105, 148, 182, 231, 245, 266,
                86, 123, 145, 145, 180, 218, 237, 237, 237,
                70, 113, 126, 166, 180, 224, 245, 245, 245, 245, 245, 245
            });


            // Кнопки меню
            SetPushi(scr, 0x0162, 0x32 - 10); // Положение окна по X
            SetPushi(scr, 0x0182, 0x3c + 9); // X кнопки Сначала
            SetPushi(scr, 0x0194, 0x78 + 2); // X кнопки Продолжить
            SetPushi(scr, 0x01a6, 0xb3 + 12); // X кнопки Выход
        }

        private void Patch625()
        {
            SetHeap(625, 3, "табл"); // pills
            SetHeap(625, 5, "порошков"); // powders
            SetHeap(625, 6, "порошок"); // powder
            SetHeap(625, 8, "распл. серебро"); // molten silver
            SetHeap(625, 9, "с медальоном"); // w/ medallion
            SetHeap(625, 10, "мл"); // ml
            SetHeap(625, 11, "грамм"); // gm
        }

        private void Patch740()
        {
            var res = _translate.GetResource<ResScript>(740);
            var scr = res.GetScript() as Script1;

            SetPushi(scr, 0x0899, 0x12 + 12); // Y второй кнопки
            SetPushi(scr, 0x08ac, 0x30 + 12); // Y третьей кнопки
        }

        private void Patch790()
        {
            SetHeap(790, 1, "Режиссёр");

            var res = _translate.GetResource<ResScript>(790);
            var scr = res.GetScript() as Script1;
            SetPushi(scr, 0x025a, 0x32 - 15); // Позиция окна X
            SetPushi(scr, 0x027a, 0x3c + 10); // X Restart
            SetPushi(scr, 0x028c, 0x78 + 4); // X Continue
            SetPushi(scr, 0x029f, 0xb3 + 15); // X Quit
        }
    }
}
