using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using System.Drawing;

namespace SCI_Tools
{
    // Русификация EqoQuest CD
    [Command("patch_eq_cd", Description = "")]
    internal class PatchEQCD : PatchCommand
    {
        protected override void Patch()
        {
            Patch822();
            /*Parch140();
            Patch360();
            Patch816();*/

            //Pic360();

            ReplaceToTransparent(40);
            ReplaceToTransparent(364);
            ReplaceToTransparent(377);
            ReplaceToTransparent(442);

            Save();
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
