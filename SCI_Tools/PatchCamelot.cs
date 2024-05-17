using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Linq;

namespace SCI_Tools
{
    // Русификация Conquests of Camelot
    [Command("patch_camelot", Description = "")]
    class PatchCamelot : PatchCommand
    {
        protected override void Patch()
        {
            PatchScript3();
            PatchScript6();
            PatchScript16();
            PatchScript58();
            PatchScript92();
            PatchScript110();
            PatchScript133();
            Save();
        }

        private void PatchScript3()
        {
            var res = _translate.GetResource<ResScript>(3);
            var scr = res.GetScript() as Script;
            SetPushi(scr, 0x1b2, 165);
        }

        private void PatchScript6()
        {
            var res = _translate.GetResource<ResScript>(6);
            var scr = res.GetScript() as Script;

            // Заменяем порядок проверок, чтобы работал запрос "спроси мага про послание розы"
            var bnt = scr.GetOperator(0xccb);
            if (bnt.Name != "bnt") throw new Exception();
            var r = bnt.Arguments[0] as CodeRef;
            if (r.TargetOffset == 0x0cda)
            {
                r.TargetOffset = 0x0d29;
                r.SetupByOffset();
                Changed(res);
            }

            bnt = scr.GetOperator(0x0ced);
            if (bnt.Name != "bnt") throw new Exception();
            r = bnt.Arguments[0] as CodeRef;
            if (r.TargetOffset == 0x0d29)
            {
                r.TargetOffset = 0x0d40;
                r.SetupByOffset();
                Changed(res);
            }

            bnt = scr.GetOperator(0x0d31);
            if (bnt.Name != "bnt") throw new Exception();
            r = bnt.Arguments[0] as CodeRef;
            if (r.TargetOffset == 0x0d40)
            {
                r.TargetOffset = 0x0cda;
                r.SetupByOffset();
                Changed(res);
            }
        }

        private void PatchScript16()
        {
            var res = _translate.GetResource<ResScript>(16);
            var scr = res.GetScript() as Script;

            SetPushi(scr, 0x195d, 65);  // x = 70
            SetPushi(scr, 0x1963, 210); // w = 200
        }

        private void PatchScript58()
        {
            var res = _translate.GetResource<ResScript>(58);
            var scr = res.GetScript() as Script;
            var op = scr.GetOperator(0xfa6);
            if (op.Name != "calle") return;
            op.Type = 0x40;
            op.Arguments.Clear();
            short val = (short)(0x2b6 - op.Address - 4);
            op.Arguments.Add(new CodeRef(op, (ushort)(op.Address + 1), (ushort)val, 0x2b6, 2));
            op.AddByte(4);

            //var op2 = scr.GetOperator(0xf8f);

            Changed(scr.Resource);
        }

        private void PatchScript92()
        {
            var res = _translate.GetResource<ResScript>(92);
            var scr = res.GetScript() as Script;

            SetPushi(scr, 0x1b62, 210); // область КОНЕЦ
            SetPushi(scr, 0x1c6b, 76);  // x "фоновые художники"
            SetPushi(scr, 0x1f3c, 60);  // x1 область "особая благодарность"
            SetPushi(scr, 0x1f40, 280); // x2 область "особая благодарность"
            SetPushi(scr, 0x1f51, 68);  // x "особая благодарность"
            SetPushi(scr, 0x1f55, 230); // width
            SetPushi(scr, 0x1f75, 60);  // x1 область "особая благодарность"
            SetPushi(scr, 0x1f79, 280); // x2 область "особая благодарность"
        }

        const string asm_cp866_to_upper = @"
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

        private void PatchScript110()
        {
            var res = _translate.GetResource<ResScript>(110);
            var scr = res.GetScript() as Script;

            var code = scr.Sections[1] as CodeSection;
            if (code.Operators.Count != 13) return;

            code.ReplaceASM(asm_cp866_to_upper);

            if (scr.GetOperator(0x768).Arguments[0] is ShortArg)
                return;

            ReplaceLdiBtoW(scr, 0x768, 0x9b); // S -> Ы
            ReplaceLdiBtoW(scr, 0x776, 0x82); // D -> В
            ReplaceLdiBtoW(scr, 0x784, 0x93); // E -> У
            ReplaceLdiBtoW(scr, 0x792, 0x80); // F -> А
        }

        private void ReplaceLdiBtoW(Script scr, ushort addr, short val)
        {
            var op = scr.GetOperator(addr);
            if (op.Name != "ldi") throw new Exception();

            if (op.Arguments[0] is ShortArg s)
                if (s.Value == val) return;

            op.Arguments[0] = new ShortArg(op, 0, val);
            op.Type = 0x34; // байты в SCI знаковые, поэтому меняем оператор на ldi W с 2-байтовым аргументом

            Changed(scr.Resource);
        }

        private void PatchScript133()
        {
            var res = _translate.GetResource<ResScript>(133);
            var scr = res.GetScript() as Script;

            if (scr.Sections.FirstOrDefault(s => s.Address == 0x4d6) is not CodeSection code) throw new Exception();
            if (code.Operators.Count != 1059) return;

            ReplaceLdiBtoW(scr, 0x5f5, 0x8a); // R => К(рус)

            var proc_addr = code.AppendASM(asm_cp866_to_upper);

            var op = scr.GetOperator(0x053c);
            var push = op.InjectNext(0x36); // push

            var rel_addr = proc_addr - (op.Address + op.Size + push.Size) + 1;
            push.InjectNext(0x40, (ushort)rel_addr, (byte)2);

            Changed(scr.Resource);
        }

    }
}
