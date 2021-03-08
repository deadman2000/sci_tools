using McMaster.Extensions.CommandLineUtils;
using SCI_Lib;
using SCI_Lib.Resources;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SCI_Tools
{
    // pack -s D:\Projects\SCI_Translator\Conquest\TRANSLATE\ -d D:\Dos\GAMES\Conquest_rus\
    [Command("pack", Description = "Package game resources")]
    class Pack : BaseCommand
    {
        [Option(Description = "Source game directory path", ShortName = "s", LongName = "source")]
        [Required]
        public string SourceDir { get; set; }

        [Option(Description = "Destination directory path", ShortName = "d", LongName = "dest")]
        [Required]
        public string DestDir { get; set; }

        protected override Task Execute()
        {
            var package = SCIPackage.Load(SourceDir);
            package.Pack(DestDir);
            return Task.CompletedTask;
        }
    }
}
