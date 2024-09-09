using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using System;

namespace SCI_Tools
{
    // Русификация Police Quest II
    [Command("patch_pq2", Description = "")]
    class PatchPQ2 : PatchCommand
    {
        protected override void Patch()
        {
            Patch4();
            Patch8();
            Patch15();
            Patch25();
            Patch153();
            Patch200();
            Patch996();
        }


        private void Patch4()
        {
            var res = _translate.GetResource<ResScript>(4);
            var scr = res.GetScript() as Script;

            // Исправление проверки пробирки крови
            {
                var op = scr.GetOperator(0x1164);
                if (op.Name != "pushi") throw new Exception();
                if (op.GetShort(0) != 146)
                {
                    op.SetShort(0, 146);
                    Changed(res);
                }
            }
            {
                var op = scr.GetOperator(0x11f9);
                if (op.Name != "pushi") throw new Exception();
                if (op.GetShort(0) != 146)
                {
                    op.SetShort(0, 146);
                    Changed(res);
                }
            }

            // Исправление переходов при проверке флагов результатов экспертизы
            foreach (var adr in new ushort[] { 0x120b, 0x1221, 0x1237, 0x124d, 0x1262 })
            {
                var op = scr.GetOperator(adr);
                if (op.Type == 0) continue;
                if (op.Name != "jmp") throw new Exception();
                op.Type = 0;
                op.Arguments.Clear();

                op.InjectNext(0);
                op.InjectNext(0);

                Changed(res);
            }
        }

        // Ввод на русском языке в компьютере
        private void Patch8()
        {
            var res = _translate.GetResource<ResScript>(8);
            var scr = res.GetScript() as Script;

            { // Фильтр символов
                var ldi = scr.GetOperator(0x743);
                if (ldi.Name != "ldi") throw new Exception();
                if (ldi.Arguments[0] is not ShortArg)
                {
                    ldi.Type = 0x34;
                    ldi.Arguments[0] = new ShortArg(ldi, 0, 0xff);

                    Changed(res);
                }
            }

            if (scr.Sections.Count == 17)
            {
                var addr = scr.AppendASM(asm_cp866_to_upper);

                var op = scr.GetOperator(0x75c);
                if (op.Name != "call") return;
                op.Arguments.Clear();
                short val = (short)(addr - op.Address - 4);
                op.Arguments.Add(new CodeRef(op, (ushort)(op.Address + 1), (ushort)val, addr, 2));
                op.AddByte(2);

                Changed(res);
            }
        }

        // Ширина окна "Боже!"
        private void Patch15()
        {
            var res = _translate.GetResource<ResScript>(15);
            var scr = res.GetScript() as Script;

            { // Фильтр символов
                var push = scr.GetOperator(0xa76);
                if (push.Name != "pushi") throw new Exception();
                if (push.GetByte(0) != 34)
                {
                    push.SetByte(0, 34);
                    Changed(res);
                }
            }
        }

        // осмотри ордер на обыск
        private void Patch25()
        {
            var res = _translate.GetResource<ResScript>(25);
            var scr = res.GetScript() as Script;

            var bnt = scr.GetOperator(0x5c5);
            if (bnt.Name != "bnt") throw new Exception();
            var r = bnt.Arguments[0] as CodeRef;
            if (r.Reference.Address != 0x62c)
            {
                r.Reference = scr.GetOperator(0x62c);
                Changed(res);
            }
        }

        // Ввод на русском языке
        private void Patch996()
        {
            var res = _translate.GetResource<ResScript>(996);
            var scr = res.GetScript() as Script;
            var ldi = scr.GetOperator(0xdd);
            if (ldi.Name != "ldi") throw new Exception();
            if (ldi.Arguments[0] is ShortArg) return;

            ldi.Type = 0x34;
            ldi.Arguments[0] = new ShortArg(ldi, 0, 0xff);
            Changed(res);
        }

        // Осмотр полевого набора. Размеры окна с текстом
        private void Patch153()
        {
            short newX = 184 - 50;
            short newW = 110 + 77;

            var res = _translate.GetResource<ResScript>(153);
            var scr = res.GetScript() as Script;
            {
                var opX = scr.GetOperator(0x138);
                if (opX.Name != "pushi") throw new Exception();
                if (opX.GetShort(0) != newX)
                {
                    opX.SetShort(0, newX);
                    Changed(res);
                }
            }
            {
                var opW = scr.GetOperator(0x143);
                if (opW.Name != "pushi") throw new Exception();
                if (opW.Arguments[0] is not ShortArg || opW.GetShort(0) != newW)
                {
                    opW.Type = 0x38;
                    opW.Arguments[0] = new ShortArg(opW, 0, newW);
                    Changed(res);
                }
            }
        }

        private void Patch200()
        {
            var res = _translate.GetResource<ResScript>(200);
            var scr = res.GetScript() as Script;

            // Изменение положения текста во вступлении на карточке Уоллса
            SetPushi(scr, 0x108c, 120); // Псевдоним
            SetPushi(scr, 0x109d, 120); // Деятельность
            SetPushi(scr, 0x10ae, 140); // дизайнер
            SetPushi(scr, 0x10c0, 120); // Полицейская

            // Художники аниматоры
            SetPushi(scr, 0x2138, 96); // art.x
            SetPushi(scr, 0x213a, 32); // art.y
            SetPushi(scr, 0x2166, 96); // animation x

            // системные разработчики
            SetPushi(scr, 0x1d91, 158); // system.x
        }

        const string asm_cp866_to_upper = @"
  ; проверяем на английский
  pushi 5a   ; 'Z'
  lap 1
  ge?        ; 5a >= p1
  bt label_ret

  pprev     ; push (p1)
  ldi 7a    ; 'z'
  le?       ; p1 <= 'z'
  bnt label_ru

  lsp 1    ; push(p1)
  ldi 20   ; acc = 20
  sub      ; acc = p1 - 20
  ret

label_ru:
  ; проверяем а-п (a0..af)
  lsp 1    ; push(p1)
  ldi a0   ; acc = a0
  ge?
  bnt label_ret
	   
  lsp 1
  ldi af
  le?
  bnt label_p
   
  lsp 1    ; push p1
  ldi 20   ; acc = 20
  sub      ; acc = pop - acc (p1 - 20)
  ret
	   
label_p: ; проверяем р-я (e0..ef)
  lsp 1
  ldi e0
  ge?
  bnt label_ret
	   
  lsp 1
  ldi ef
  le?
  bnt label_ret
	   
  lsp 1
  ldi 50
  sub      ; acc = pop - acc (p1 - 50)
  ret
	   
label_ret:
  lap 1    ; acc = p1
  ret
";
    }
}
