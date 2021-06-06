using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SCI_Tools
{
    [Command("extract_view", Description = "Extract view into png")]
    class ExtractView : PackageCommand
    {
        [Option(Description = "View number", LongName = "view", ShortName = "v")]
        public ushort? Picture { get; set; }

        [Option(Description = "Output directory", LongName = "out_dir", ShortName = "o")]
        public string OutDir { get; set; } = "./view";

        protected override Task Do()
        {
            if (!Directory.Exists(OutDir))
                Directory.CreateDirectory(OutDir);

            if (Picture.HasValue)
            {
                var v = package.GetResource<ResView>(Picture.Value);
                Save(v);
            }
            else
            {
                var views = package.GetResources<ResView>();
                foreach (var v in views)
                    Save(v);
            }

            return Task.CompletedTask;
        }

        private void Save(ResView v)
        {
            try
            {
                var view = v.GetView();

                for (int l = 0; l < view.Loops.Count; l++)
                {
                    for (int c = 0; c < view.Loops[l].Cells.Count; c++)
                    {
                        var cell = view.Loops[l].Cells[c];
                        cell.GetImage().Save(Path.Combine(OutDir, $"{v.Number}.{l}.{c}.png"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"View {v.Number} {ex.Message}");
            }
        }
    }
}
