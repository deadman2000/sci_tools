using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts1_1;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SCI_Tools
{
    // test -d D:\Dos\GAMES\QG_EGA\
    [Command("test", Description = "for testing")]
    class Test : PackageCommand
    {
        protected override Task Do()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return Task.CompletedTask;
        }

    }
}
