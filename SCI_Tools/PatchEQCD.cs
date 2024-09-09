using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts1;
using System.Drawing;

namespace SCI_Tools
{
    // Русификация EqoQuest CD
    [Command("patch_eq_cd", Description = "")]
    internal class PatchEQCD : PatchCommand
    {
        protected override void Patch()
        {
            Parch140();
            Patch816();
            Patch822();
            //Pic360();

            ReplaceToTransparent(40);
            ReplaceToTransparent(364);
            ReplaceToTransparent(377);
            ReplaceToTransparent(442);
        }

        private void Parch140()
        {
            var res = _translate.GetResource<ResScript>(140);
            var scr = res.GetScript() as Script1;

            // Размер кнопки "Помощь"
            // proc_2c71  140 -> 160
            SetPushi(scr, 0x0958, 160);
            SetPushi(scr, 0x1878, 160);
        }

        private void Patch816()
        {
            var res = _translate.GetResource<ResScript>(816);
            var scr = res.GetScript();
            // Display 10
            SetPushi(scr, 0x03f2, 55); // Нажми на Свиток, чтобы прокрутить.          67->55
            SetPushi(scr, 0x040b, 36); // Щёлкни рядом со Свитком, чтобы закрыть.     66->36
            SetPushi(scr, 0x044a, 70); // Для прокрутки нажми стрелки вверх или вниз. 46->70
        }

        private void Patch822()
        {
            var res = _translate.GetResource<ResScript>(822);
            var resH = _translate.GetResource<ResHeap>(822);
            var scr = res.GetScript();

            var iconRestore = scr.GetInstance("iconRestore");
            if (iconRestore.GetProperty("nsLeft") != 64)
            {
                iconRestore.SetProperty("nsLeft", 64);
                Changed(resH);
            }

            var iconQuit = scr.GetInstance("iconQuit");
            if (iconQuit.GetProperty("nsLeft") != 137)
            {
                iconQuit.SetProperty("nsLeft", 137);
                Changed(resH);
            }
        }

        private void ReplaceToTransparent(ushort num)
        {
            bool changed = false;
            var res = _translate.GetResource<ResView>(num);
            var view = res.GetView();

            foreach (var loop in view.Loops)
            {
                foreach (var cell in loop.Cells)
                {
                    for (int i = 0; i < cell.Pixels.Length; i++)
                    {
                        if (cell.Pixels[i] == 0)
                        {
                            cell.Pixels[i] = cell.TransparentColor;
                            changed = true;
                        }
                    }
                }
            }

            if (changed)
            {
                res.SetView(view);
                Changed(res);
            }
        }

        private void Pic360()
        {
            var bmp = new Bitmap(@"D:\Projects\TranslateWeb\EQ\CD\360_ru2.bmp");

            var res = _translate.GetResource<ResPicture>(360);
            var pic = res.GetPicture();
            //pic.GetBackground().Save(@"D:\Projects\TranslateWeb\EQ\CD\360_orig.bmp");
            pic.SetBackgroundIndexed(bmp);
            res.SetPicture(pic);
            res.SavePatch();
        }

    }
}
