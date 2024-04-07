using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
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
            Patch360();

            //Pic360();
            //Font310();

            Save();
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
            pic.SetBackground(bmp);
            res.SetPicture(pic);
            res.SavePatch();
        }

        private void Patch0()
        {
            var res = _translate.GetResource<ResScript>(0);
            var scr = res.GetScript() as Script;

            var iconRestore = scr.GetInstance("iconRestore");
            if (iconRestore.Properties[7].Value != 64)
            {
                iconRestore.Properties[7].Value = 64;
                Changed(res);
            }

            var iconAbout = scr.GetInstance("iconAbout");
            if (iconAbout.Properties[7].Value != 64)
            {
                iconAbout.Properties[7].Value = 64;
                Changed(res);
            }

            var iconHelp = scr.GetInstance("iconHelp");
            if (iconHelp.Properties[7].Value != 100)
            {
                iconHelp.Properties[7].Value = 100;
                Changed(res);
            }

            var iconQuit = scr.GetInstance("iconQuit");
            if (iconQuit.Properties[7].Value != 136)
            {
                iconQuit.Properties[7].Value = 136;
                Changed(res);
            }
        }

        private void Patch360()
        {
            var res = _translate.GetResource<ResScript>(360);
            var scr = res.GetScript() as Script;

            var presents = scr.GetInstance("presents");
            ushort val = 32;
            if (presents.Properties[4].Value != val)
            {
                presents.Properties[4].Value = val;
                Changed(res);
            }
        }
    }
}
