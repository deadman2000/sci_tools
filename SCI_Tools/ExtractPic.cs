using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SCI_Tools
{
    [Command("extract_pic", Description = "Extract picture into bmp")]
    class ExtractPic : PackageCommand
    {
        [Option(Description = "Picture number", LongName = "picture", ShortName = "p")]
        public ushort? Picture { get; set; }

        [Option(Description = "Output directory", LongName = "out_dir", ShortName = "o")]
        public string OutDir { get; set; } = "./pic";

        protected override Task Do()
        {
            if (!Directory.Exists(OutDir))
                Directory.CreateDirectory(OutDir);

            if (Picture.HasValue)
            {
                var p = package.GetResource<ResPicture>(Picture.Value);
                Save(p);
            }
            else
            {
                var pics = package.GetResources<ResPicture>();
                foreach (var p in pics)
                    Save(p);
            }

            return Task.CompletedTask;
        }

        private void Save(ResPicture p)
        {
            try
            {
                var pic = p.GetPicture();
                var bgr = pic.GetBackground();
                if (bgr != null)
                    bgr.Save(Path.Combine(OutDir, $"{p.Number}.bmp"));
                else
                    Console.WriteLine($"Pic {p.Number} has no image");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Pic {p.Number} {ex.Message}");
            }
        }
    }
}
