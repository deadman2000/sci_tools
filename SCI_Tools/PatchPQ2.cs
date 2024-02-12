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
            Patch8();
            Patch15();
            Patch996();
            Patch153();
            Save();
        }

        // Ввод на русском языке в компьютере
        private void Patch8()
        {
            var res = _translate.GetResource<ResScript>(8);
            var scr = res.GetScript() as Script;

            { // Фильтр символов
                var ldi = scr.GetOperator(0x743);
                if (ldi.Name != "ldi") throw new Exception();
                if (ldi.Arguments[0] is not ushort)
                {
                    ldi.Type = 0x34;
                    ldi.Arguments[0] = (ushort)0xff;

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
                op.Arguments.Add((byte)2);

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
                if ((byte)push.Arguments[0] != 34)
                {
                    push.Arguments[0] = (byte)34;
                    Changed(res);
                }
            }
        }

        // Ввод на русском языке
        private void Patch996()
        {
            var res = _translate.GetResource<ResScript>(996);
            var scr = res.GetScript() as Script;
            var ldi = scr.GetOperator(0xdd);
            if (ldi.Name != "ldi") throw new Exception();
            if (ldi.Arguments[0] is ushort) return;

            ldi.Type = 0x34;
            ldi.Arguments[0] = (ushort)0xff;
            Changed(res);
        }

        // Осмотр полевого набора. Размеры окна с текстом
        private void Patch153()
        {
            ushort newX = 184 - 50;
            ushort newW = 110 + 77;

            var res = _translate.GetResource<ResScript>(153);
            var scr = res.GetScript() as Script;
            {
                var opX = scr.GetOperator(0x138);
                if (opX.Name != "pushi") throw new Exception();
                if ((ushort)opX.Arguments[0] != newX)
                {
                    opX.Arguments[0] = newX;
                    Changed(res);
                }
            }
            {
                var opW = scr.GetOperator(0x143);
                if (opW.Name != "pushi") throw new Exception();
                if (opW.Arguments[0] is not ushort || (ushort)opW.Arguments[0] != newW)
                {
                    opW.Type = 0x38;
                    opW.Arguments[0] = newW;
                    Changed(res);
                }
            }
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
