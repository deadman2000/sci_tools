using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCI_Tools
{
    // Русификация EqoQuest CD
    [Command("patch_eq_cd", Description = "")]
    internal class PatchEQCD : PatchCommand
    {
        protected override void Patch()
        {
            /*Patch0();
            Parch140();
            Patch360();
            Patch816();*/

            //Pic360();

            Save();
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
