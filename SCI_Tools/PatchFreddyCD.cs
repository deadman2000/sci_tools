using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts1;
using System;
using System.Linq;

namespace SCI_Tools
{
    // Русификация Freddy Pharkas CD
    [Command("patch_fp_cd", Description = "")]
    internal class PatchFreddyCD : PatchCommand
    {
        protected override void Patch()
        {
            AddMessages240();
            AddMessages250();
            AddMessages320();
            AddMessages600();
            AddMessages660();
            Patch14();
            Patch90();
            Patch110();
            Patch150();
            Patch220();
            Patch625();
            Patch660();
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

        private void Patch14()
        {
            // Субтитры и речь при запуске игры
            var res = _translate.GetResource<ResScript>(14);
            var scr = res.GetScript() as Script1;

            /*var op = scr.GetOperator(0x005e);
            if (op.Name != "push2") throw new Exception();
            op.Type = 0x39;
            op.Arguments.Add(new ByteArg(op, 0, 3));*/

            var op = scr.GetOperator(0x005a);
            if (op.Name != "ldi") throw new Exception();
            var arg = (ByteArg)op.Arguments[0];
            if (arg.Value != 3)
            {
                arg.Value = 3;
                Changed(scr.Resource);
            }
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
        }

        private void Patch150()
        {
            // Кнопки меню в прологе
            var res = _translate.GetResource<ResScript>(150);
            var scr = res.GetScript() as Script1;

            SetPushi(scr, 0x0594, 0x2e - 22); // Положение окна по X
            SetPushi(scr, 0x05b8, 0x37 + 10); // X кнопки Пролог
            SetPushi(scr, 0x05c9, 0x6e + 4); // X кнопки Играть
            SetPushi(scr, 0x05da, 0x95 + 12); // X кнопки Помощь
            SetPushi(scr, 0x05eb, 0xbc + 22); // X кнопки Выход
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
        }

        private void Patch625()
        {
            /* 3 : pills
            5 : powders
            6 : powder
            8 : molten silver
            9 : w/ medallion
            10 : ml
            11 : gm*/

            SetHeap(625, 3, "табл");
            SetHeap(625, 5, "порошков");
            SetHeap(625, 6, "порошок");
            SetHeap(625, 8, "распл. серебро");
            SetHeap(625, 9, "с медальоном");
            SetHeap(625, 10, "мл");
            SetHeap(625, 11, "грамм");
        }

        private void Patch660()
        {
            var res = _translate.GetResource<ResScript>(660);
            var scr = res.GetScript() as Script1;

            SetPushi(scr, 0x06ad, 39); // Замена проблемных msg на новые
            SetPushi(scr, 0x06de, 39); // Замена проблемных msg на новые
            SetLdi(scr, 0x06d4, 4); // Хоп Сингх отвечает на правильную фразу
        }
    }
}
