using McMaster.Extensions.CommandLineUtils;
using SCI_Lib;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SCI_Tools
{
    abstract class PackageCommand : BaseCommand
    {
        [Option(Description = "Original game directory", ShortName = "d", LongName = "dir")]
        [Required]
        public string GameDir { get; set; }

        [Option(Description = "Translated game directory", ShortName = "t", LongName = "trans")]
        public string TranslateDir { get; set; }

        protected SCIPackage package;

        protected SCIPackage translate;

        protected override async Task Execute()
        {
            package = SCIPackage.Load(GameDir);
            if (!string.IsNullOrEmpty(TranslateDir))
                translate = SCIPackage.Load(TranslateDir);

            await Do();
        }

        protected abstract Task Do();
    }
}
