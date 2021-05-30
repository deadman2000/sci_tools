using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SCI_Tools
{
    // export_font -d D:\Projects\SCI_Translator\Conquest\ -f 0
    [Command("export_font", Description = "Export font to images")]
    class FontExport : PackageCommand
    {
        [Option(Description = "Font number", LongName = "font", ShortName = "f")]
        public ushort Font { get; set; }

        protected override Task Do()
        {
            var res = package.GetResource<ResFont>(Font);
            var outDir = Path.GetFullPath(res.FileName);
            if (Directory.Exists(outDir))
                Directory.Delete(outDir, true);
            Directory.CreateDirectory(outDir);

            var font = res.GetFont();
            for (int i = 0x20; i < font.Frames.Count; i++)
            {
                var f = font.Frames[i];
                Console.WriteLine($"{(char)i}: {f.Width}x{f.Height}");
                f.ExportToImage(Path.Combine(outDir, $"{i:X2}.png"));
            }

            return Task.CompletedTask;
        }
    }
}
