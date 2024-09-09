using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Sections;
using System.Drawing;

namespace SCI_Tools
{
    // Русификация EqoQuest
    [Command("patch_eq", Description = "")]
    internal class PatchEQ : PatchCommand
    {
        protected override void Patch()
        {
            Patch0();
            Parch140();
            Patch360();
            Patch816();

            //Pic360();
            //Font310();
        }

        private void Font310()
        {
            var fnt300 = _translate.GetResource<ResFont>(300).GetFont();

            var res = _translate.GetResource<ResFont>(310);
            var fnt = res.GetFont();

            for (int i = 128; i < fnt300.Frames.Count; i++)
            {
                var f = fnt[i] = fnt300[i].Clone();
                if (f.Width > 1 && f.Height > 1)
                {
                    f.Resize(f.Width + 1, f.Height + 1);
                    f.ShiftDown();
                }
            }

            Changed(res);
        }

        private void Pic360()
        {
            var bmp = new Bitmap(@"D:\Projects\TranslateWeb\EQ\out.bmp");

            var res = _translate.GetResource<ResPicture>(360);
            var pic = res.GetPicture();
            pic.SetBackgroundIndexed(bmp);
            res.SetPicture(pic);
            res.SavePatch();
        }

        private void Patch0()
        {
            var res = _translate.GetResource<ResScript>(0);
            var scr = res.GetScript();

            var iconRestore = scr.GetInstance("iconRestore");
            if (iconRestore.GetProperty("nsLeft") != 64)
            {
                iconRestore.SetProperty("nsLeft", 64);
                Changed(res);
            }

            var iconAbout = scr.GetInstance("iconAbout");
            if (iconAbout.GetProperty("nsLeft") != 64)
            {
                iconAbout.SetProperty("nsLeft", 64);
                Changed(res);
            }

            var iconHelp = scr.GetInstance("iconHelp");
            if (iconHelp.GetProperty("nsLeft") != 100)
            {
                iconHelp.SetProperty("nsLeft", 100);
                Changed(res);
            }

            var iconQuit = scr.GetInstance("iconQuit");
            if (iconQuit.GetProperty("nsLeft") != 136)
            {
                iconQuit.SetProperty("nsLeft", 136);
                Changed(res);
            }
        }

        private void Parch140()
        {
            var res = _translate.GetResource<ResScript>(140);
            var scr = res.GetScript();

            // Размер кнопки "Помощь"
            // 140 -> 160   proc_4456
            SetPushi(scr, 0x0873, 160);
            SetPushi(scr, 0x2508, 160);
        }

        private void Patch360()
        {
            var res = _translate.GetResource<ResScript>(360);
            var scr = res.GetScript();

            var presents = scr.GetInstance("presents") as ClassSection;
            ushort val = 32;
            if (presents.Properties[4].Value != val)
            {
                presents.Properties[4].Value = val;
                Changed(res);
            }
        }

        private void Patch816()
        {
            var res = _translate.GetResource<ResScript>(816);
            var scr = res.GetScript();
            // Display 10
            SetPushi(scr, 0x02ad, 55); // Нажми на Свиток, чтобы прокрутить.          67->55
            SetPushi(scr, 0x02c6, 36); // Щёлкни рядом со Свитком, чтобы закрыть.     66->36
            SetPushi(scr, 0x0302, 70); // Для прокрутки нажми стрелки вверх или вниз. 46->70
            //SetPushi(scr, 0x031b, 83); // Чтобы закрыть, нажми ESC.
        }
    }
}
