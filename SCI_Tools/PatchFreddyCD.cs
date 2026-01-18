using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts1;
using System.Linq;

namespace SCI_Tools
{
    // Русификация Freddy Pharkas CD
    [Command("patch_fp_cd", Description = "")]
    internal class PatchFreddyCD : PatchCommand
    {
        protected override void Patch()
        {
            AddMessages220();
            AddMessages240();
            AddMessages250();
            AddMessages320();
            AddMessages600();
            AddMessages660();

            PatchMessages110();
            PatchMessages220();
            PatchMessages235();
            PatchMessages260();
            PatchMessages300();
            PatchMessages570();
            PatchMessages650();

            Patch14();
            Patch15();
            Patch24();
            Patch25();
            Patch90();
            Patch110();
            Patch150();
            Patch170();
            Patch220();
            Patch265();
            Patch625();
            Patch650();
            Patch660();
            Patch670();
            Patch720();
            Patch730();
            Patch740();
            Patch790();
        }

        private void AddMessages220()
        {
            var res = _translate.GetResource<ResMessage>(220);
            AddMessage(res, noun: 42, verb: 0, cond: 0, seq: 1, talker: 99);
            AddMessage(res, noun: 42, verb: 0, cond: 0, seq: 9, talker: 55);
            AddMessage(res, noun: 42, verb: 0, cond: 0, seq: 10, talker: 55);
            AddMessage(res, noun: 42, verb: 0, cond: 0, seq: 11, talker: 49);
            AddMessage(res, noun: 42, verb: 0, cond: 0, seq: 12, talker: 99);

            AddMessage(res, noun: 43, verb: 0, cond: 0, seq: 3, talker: 99);
            AddMessage(res, noun: 43, verb: 0, cond: 0, seq: 4, talker: 99);
            AddMessage(res, noun: 43, verb: 0, cond: 0, seq: 5, talker: 99);
        }

        private void AddMessages240()
        {
            var res = _translate.GetResource<ResMessage>(240);
            AddMessage(res, noun: 8, verb: 4, cond: 0, seq: 2, talker: 99);
            AddMessage(res, noun: 8, verb: 4, cond: 0, seq: 3, talker: 99);
            AddMessage(res, noun: 8, verb: 4, cond: 0, seq: 4, talker: 99);
        }

        private void AddMessages250()
        {
            var res = _translate.GetResource<ResMessage>(250);
            AddMessage(res, noun: 25, verb: 0, cond: 0, seq: 2, talker: 57);
        }

        private void AddMessages320()
        {
            var res = _translate.GetResource<ResMessage>(320);
            AddMessage(res, noun: 9, verb: 1, cond: 0, seq: 2, talker: 99);
        }

        private void AddMessages600()
        {
            var res = _translate.GetResource<ResMessage>(600);

            var msgs = res.GetMessages();
            var msg = msgs.First(m => m.Noun == 6 && m.Verb == 1 && m.Cond == 0 && m.Seq == 2);
            if (msg.Talker != 99)
            {
                msg.Talker = 99;
                Changed(res);
            }

            AddMessage(res, noun: 6, verb: 1, cond: 0, seq: 3, talker: 49);
            AddMessage(res, noun: 20, verb: 1, cond: 0, seq: 2, talker: 99);
            AddMessage(res, noun: 15, verb: 1, cond: 0, seq: 2, talker: 99);
        }

        private void AddMessages660()
        {
            var res = _translate.GetResource<ResMessage>(660);
            AddMessage(res, noun: 39, verb: 0, cond: 0, seq: 1, talker: 53);
            AddMessage(res, noun: 39, verb: 0, cond: 0, seq: 2, talker: 53);
            AddMessage(res, noun: 39, verb: 0, cond: 0, seq: 3, talker: 53);
            AddMessage(res, noun: 39, verb: 0, cond: 0, seq: 4, talker: 53);
            AddMessage(res, noun: 39, verb: 0, cond: 0, seq: 5, talker: 53);
            AddMessage(res, noun: 39, verb: 0, cond: 0, seq: 6, talker: 12);
        }


        private void PatchMessages110()
        {
            // Фикс порядка сообщений
            var res = _translate.GetResource<ResMessage>(110);
            var messages = res.GetMessages();
            if (messages[26].Seq != 2)
            {
                messages[26].Seq = 2;
                messages[27].Seq = 3;
                Changed(res);
            }
        }

        private void PatchMessages220()
        {
            // Фикс порядка сообщений
            var res = _translate.GetResource<ResMessage>(220);
            var messages = res.GetMessages();
            if (messages[82].Seq != 2)
            {
                messages[82].Seq = 2;
                messages[83].Seq = 3;
                Changed(res);
            }
        }

        private void PatchMessages235()
        {
            // Фикс порядка сообщений
            var res = _translate.GetResource<ResMessage>(235);
            var messages = res.GetMessages();
            if (messages[8].Seq != 2)
            {
                messages[8].Seq = 2;
                Changed(res);
            }

            if (messages[18].Seq != 2)
            {
                for (int i = 18; i <= 21; i++)
                {
                    messages[i].Seq--;
                }
                Changed(res);
            }
        }

        private void PatchMessages260()
        {
            // Фикс порядка сообщений
            var res = _translate.GetResource<ResMessage>(260);
            var messages = res.GetMessages();
            {
                if (messages[159].Seq != 8)
                {
                    messages[159].Seq = 8;
                    messages[160].Seq = 9;
                    Changed(res);
                }
            }
        }

        private void PatchMessages300()
        {
            var res = _translate.GetResource<ResMessage>(300);
            var messages = res.GetMessages();
            if (messages[90].Talker != 40)
            {
                messages[90].Talker = 40;
                Changed(res);
            }
        }

        private void PatchMessages570()
        {
            var res = _translate.GetResource<ResMessage>(570);
            var messages = res.GetMessages();
            if (messages[24].Seq != 3)
            {
                messages[24].Seq = 3;
                Changed(res);
            }
        }

        private void PatchMessages650()
        {
            // Фикс порядка сообщений
            var res = _translate.GetResource<ResMessage>(650);
            var messages = res.GetMessages();
            if (messages[1].Seq != 2)
            {
                messages[1].Seq = 2;
                Changed(res);
            }

            if (messages[29].Seq != 7)
            {
                for (int i = 29; i <= 32; i++)
                {
                    messages[i].Seq--;
                }
                Changed(res);
            }

            if (messages[73].Seq != 2)
            {
                messages[73].Seq = 2;
                messages[74].Seq = 3;
                Changed(res);
            }

            if (messages[107].Seq != 2)
            {
                messages[107].Seq = 2;
                messages[108].Seq = 3;
                Changed(res);
            }
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

            SetPushi(scr, 0x0266, 0x2e - 22); // Положение окна по X
            SetPushi(scr, 0x0285, 0x37 + 10); // X кнопки Пролог
            SetPushi(scr, 0x0296, 0x6e + 4); // X кнопки Играть
            SetPushi(scr, 0x02a7, 0x95 + 12); // X кнопки Помощь
            SetPushi(scr, 0x02b8, 0xbc + 22); // X кнопки Выход

            SetLdi(scr, 0x0403, 3); // Режим текст+речь при выходе из меню Помощь
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
            SetPushi(scr, 0x0594, 0x2e - 22); // Положение окна по X
            SetPushi(scr, 0x05b8, 0x37 + 10); // X кнопки Пролог
            SetPushi(scr, 0x05c9, 0x6e + 4); // X кнопки Играть
            SetPushi(scr, 0x05da, 0x95 + 12); // X кнопки Помощь
            SetPushi(scr, 0x05eb, 0xbc + 22); // X кнопки Выход

            // Режим текст+речь при выходе из меню Помощь
            SetLdi(scr, 0x076c, 3);
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
        }

        private void Patch220()
        {
            var res = _translate.GetResource<ResScript>(220);
            var scr = res.GetScript() as Script1;

            // Фикс повторяющегося текста
            var code = scr.GetOperator(0x16b9);
            if (code.Name != "jmp")
            {
                // Меняем код на jmp, чтобы пропустить выполнение вызова текста
                code.Type = 0x32; // jmp W
                code.Arguments.Clear();
                code.Arguments.Add(new ShortArg(code, 0, 11));

                Changed(res);
            }

            // Замена диалога с братьями на новый
            SetPushi(scr, 0x0c77, 42); // 1
            SetPushi(scr, 0x0c9c, 42); // 2
            SetPushi(scr, 0x0cf8, 42); // 3
            SetPushi(scr, 0x0d70, 42); // 4
            SetPushi(scr, 0x0da2, 42); // 5
            SetPushi(scr, 0x0e3b, 42); // 6
            SetPushi(scr, 0x0e6d, 42); // 7
            SetPushi(scr, 0x0f1c, 42); // 8

            // Замена диалога с Кенни на новый
            SetPushi(scr, 0x0fc5, 43); // 3
            SetPushi(scr, 0x1057, 43); // 4..5
        }

        private void Patch265()
        {
            var res = _translate.GetResource<ResScript>(265);
            var scr = res.GetScript() as Script1;

            // Фикс правильного окна смерти при падении мешков на Фредди
            SetLdi(scr, 0x1055, 8);
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

        private void Patch650()
        {
            var res = _translate.GetResource<ResScript>(650);
            var scr = res.GetScript() as Script1;

            // Фикс диалога Вилли, в квесте с лошадьми
            var op = scr.GetOperator(0x0618);
            if (op.Name != "bt")
            {
                op.Type = 0x2f; // bt B
                var r = (RelativeByteRef)op.Arguments[0];
                r.Reference = scr.GetOperator(0x0621);
                Changed(res);
            }
        }

        private void Patch660()
        {
            var res = _translate.GetResource<ResScript>(660);
            var scr = res.GetScript() as Script1;

            SetPushi(scr, 0x06ad, 39); // Замена проблемных msg на новые
            SetPushi(scr, 0x06de, 39); // Замена проблемных msg на новые
            SetLdi(scr, 0x06d4, 4); // Хоп Сингх отвечает на правильную фразу
        }

        private void Patch670()
        {
            var res = _translate.GetResource<ResScript>(670);
            var scr = res.GetScript() as Script1;

            // Правильный диалог Исправленным рецептом на дока
            {
                // push0 -> pushi 17
                var op = scr.GetOperator(0x1362);
                if (op.Name != "pushi")
                {
                    op.Type = 0x39; // pushi B
                    op.Arguments.Add(new ByteArg(op, 0, 17));
                    Changed(res);
                }
            }
            {
                // pushi 5 -> pushi0
                var op = scr.GetOperator(0x1363);
                if (op != null && op.Name != "push0")
                {
                    op.Type = 0x76; // push0
                    op.Arguments.Clear();
                    Changed(res);
                }
            }
        }

        private void Patch720()
        {
            var res = _translate.GetResource<ResScript>(720);
            var scr = res.GetScript() as Script1;

            // Добавляем номер конечного сообщения
            if (scr.GetOperator(0x0471).Name != "push2")
            {
                SetPushi(scr, 0x046a, 6);
                scr.GetOperator(0x0470).InjectNext(0x7a); // + push2
                scr.GetOperator(0x0474).SetByte(0, 0x10); // send 10

                var op = scr.GetOperator(0x04bf);
                op.Type = 0x39; // pushi 7
                op.AddByte(3);

                SetPushi(scr, 0x04c0, 8);

                SetPushi(scr, 0x0522, 9);
                SetPushi(scr, 0x0524, 10);

                SetPushi(scr, 0x056a, 11);
                SetPushi(scr, 0x056c, 13);

                SetPushi(scr, 0x05bb, 14);
                SetPushi(scr, 0x05bd, 15);

                Changed(res);
            }

            // SetPushi(scr, 0x0607, 0xff); // Увеличить время на ожидание выстрела
        }

        private void Patch730()
        {
            var res = _translate.GetResource<ResScript>(730);
            var scr = res.GetScript() as Script1;

            // Добавляем еще одну фразу Пенелопе

            if (scr.GetOperator(0x052a).Name != "pushi")
            {
                SetPushi(scr, 0x0523, 6);
                scr.GetOperator(0x0528).InjectNext(0x39, (byte)8); // pushi 8
                scr.GetOperator(0x052d).SetByte(0, 0x10); // send 10

                SetPushi(scr, 0x0571, 9);
            }
        }

        private void Patch740()
        {
            var res = _translate.GetResource<ResScript>(740);
            var scr = res.GetScript() as Script1;

            SetPushi(scr, 0x08c8, 30); // Y второй кнопки
            SetPushi(scr, 0x08db, 60); // Y третьей кнопки
        }

        private void Patch790()
        {
            SetHeap(790, 1, "Режиссёр");

            var res = _translate.GetResource<ResScript>(790);
            var scr = res.GetScript() as Script1;
            SetPushi(scr, 0x027a, 60 + 10); // X второй кнопки
        }
    }
}
