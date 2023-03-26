using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using System.Drawing;
using System.Threading.Tasks;

namespace SCI_Tools
{
    // replace_view -v 731 -l 4 -c 0 -i D:\Projects\view.png -d D:\Dos\GAMES\DaggerAmonRa\TRANSLATE\
    [Command("replace_view", Description = "Replace view with image")]
    class ReplaceView : PackageCommand
    {
        [Option(Description = "View number", LongName = "view", ShortName = "v")]
        public ushort View { get; set; }

        [Option(Description = "Loop number", LongName = "loop", ShortName = "l")]
        public ushort Loop { get; set; }

        [Option(Description = "Cell number", LongName = "cell", ShortName = "c")]
        public ushort Cell { get; set; }

        [Option(Description = "Image path", LongName = "img", ShortName = "i")]
        public string ImagePath { get; set; }

        protected override Task Do()
        {
            var res = package.GetResource<ResView>(View);
            var view = res.GetView();

            view.Loops[Loop].Cells[Cell].SetImage((Bitmap)Image.FromFile(ImagePath));

            res.SetView(view);
            res.SavePatch();

            return Task.CompletedTask;
        }
    }
}
