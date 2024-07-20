using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts.Elements;
using System;

namespace SCI_Tools
{
    // Русификация Conquests of the Longbow
    [Command("patch_longbow", Description = "")]
    internal class PatchLongbow : PatchCommand
    {
        protected override void Patch()
        {
            Patch10();
            Patch97();
            Patch370();

            Save();
        }

        private void Patch10()
        {
            var res = _translate.GetResource<ResScript>(10);
            var scr = res.GetScript();

            // Исправление анимации падающего рыцаря
            var send = scr.GetOperator(0x1196);
            if (send == null || send.Name != "send") return;
            var op = scr.GetOperator(0x1180);

            // Вариант не запускать анимацию взмаха мечом. Не сработал в DOS, вылетает
            // acc = fKnight.cel
            /*op = op.InjectNext(0x39, (byte)7);
            op = op.InjectNext(0x76);
            op = op.InjectNext(0x72, (short)0x446);
            op = op.InjectNext(0x4a, (byte)4);

            op = op.InjectNext(0x36); // push(acc)
            op = op.InjectNext(0x35, (byte)3); // acc = 3
            op = op.InjectNext(0x1c); // ne?
            op = op.InjectNext(0x30); // bnt
            var cr = new CodeRef(op, 0, 0, 0, 2);
            cr.Shift = 2;
            cr.Reference = scr.GetOperator(0x1198);
            op.Arguments.Add(cr);*/

            // Перезапуск анимации с первого кадра
            op = op.InjectNext(0x38, (short)0x120); // push 120;  setCel
            op = op.InjectNext(0x78); // push1
            op.InjectNext(0x76); // push0
            send.SetByte(0, 0x16);

            Changed(res);
        }

        private void Patch97()
        {
            var res = _translate.GetResource<ResScript>(97);
            var scr = res.GetScript();

            const ushort leftX = 20;

            // Программситы X
            SetPushi(scr, 0x0a03, leftX);
            SetPushi(scr, 0x0a3d, leftX);
            SetPushi(scr, 0x0a76, leftX);
            SetPushi(scr, 0x0aaf, leftX);
            SetPushi(scr, 0x0b07, leftX);

            // Система разработки X
            SetPushi(scr, 0x0bf1, leftX);
            SetPushi(scr, 0x0c29, leftX);
            SetPushi(scr, 0x0c61, leftX);
            SetPushi(scr, 0x0c9a, leftX);
            SetPushi(scr, 0x0cd3, leftX);
            SetPushi(scr, 0x0d13, leftX);

            var name13 = scr.GetInstance("name13"); // Джон Крейн
            if (name13.GetProperty("x") != leftX)
            {
                name13.SetProperty("x", leftX);
                Changed(res);
            }
        }

        private void Patch370()
        {
            var res = _translate.GetResource<ResScript>(370);
            var scr = res.GetScript();

            // Ширина окна сообщения
            //                        X          WIDTH
            // (Say 1370 1 self 67 >>249<< 15 70 >>68<<)

            // localproc_0
            SetPushi(scr, 0x001a, 61 + 38);

            // localproc_1
            SetPushi(scr, 0x002e, 250 - 30);
            SetPushi(scr, 0x0035, 67 + 30);

            // Простите мою осторожность, у шерифа всюду лазутчики, я должен быть уверен.
            SetPushi(scr, 0x046e, 249 - 14);
            SetPushi(scr, 0x0475, 68 + 14);

            // Так!  Принц вступил в сговор с шерифом и аббатом ноттингемским.
            SetPushi(scr, 0x07a2, 230 - 28);
            SetPushi(scr, 0x07a9, 75 + 28);
        }
    }
}
