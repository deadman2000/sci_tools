using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SCI_Tools
{
    // tr_to_txt -d D:\Projects\SCI_Translator\Conquest\ -t D:\Projects\SCI_Translator\Conquest\TRANSLATE\
    [Command("tr_to_txt", Description = "Extract translate to text file")]
    class TranslatedToText : PackageCommand
    {
        protected override async Task Do()
        {
            var path = "translate.txt";
            File.Delete(path);
            
            var resources = package.GetTextResources();

            using var f = new StreamWriter(path);
            foreach (var res in resources)
            {
                var tres = translate.Get(res);

                Console.WriteLine(res);
                await f.WriteLineAsync(res.FileName);

                var en = res.GetStrings();
                var tr = tres.GetStrings();
                for (int i = 0; i < tr.Length; i++)
                {
                    if (en[i] != tr[i])
                        await f.WriteLineAsync(tr[i]);
                }
            }
        }
    }
}
