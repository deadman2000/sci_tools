using McMaster.Extensions.CommandLineUtils;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SCI_Tools
{
    abstract class BaseCommand
    {
        [Option(Description = "Disable read key pause", LongName = "no-wait", ShortName = "w")]
        public bool NoWait { get; set; }

        protected async Task OnExecute()
        {
            await Execute();

            if (!NoWait && !Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        protected abstract Task Execute();
    }
}
