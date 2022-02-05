using McMaster.Extensions.CommandLineUtils;
using SCI_Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SCI_Tools
{
    // apply -d D:\Dos\GAMES\QG_VGA\ -t D:\Dos\GAMES\QG_VGA_RUS\ -d2 D:\Dos\GAMES\QG_EGA\ -t2 d:\Dos\Games\QG_EGA_RUS\
    [Command("apply", Description = "Apply translate from one game to another")]
    class CompareTranslates : PackageCommand
    {
        [Option(Description = "Second original game directory", ShortName = "d2", LongName = "dir2")]
        [Required]
        public string SecondGameDir { get; set; }

        [Option(Description = "Second translated game directory", ShortName = "t2", LongName = "trans2")]
        public string SecondTranslateDir { get; set; }

        protected SCIPackage package2;
        protected SCIPackage translate2;

        protected override Task Do()
        {
            package2 = SCIPackage.Load(SecondGameDir);
            translate2 = SCIPackage.Load(SecondTranslateDir);

            Console.WriteLine("Translate gathering...");

            Dictionary<string, string> translates = new Dictionary<string, string>();
            Dictionary<string, string> translatesSimple = new Dictionary<string, string>();

            foreach (var mess in package.Messages)
            {
                var enMess = mess.GetMessages();
                var trMess = translate.Get(mess).GetMessages();
                if (enMess.Count != trMess.Count)
                {
                    Console.WriteLine($"{mess.FileName} Lines mismatch");
                    continue;
                }

                for (int i = 0; i < enMess.Count; i++)
                {
                    var en = enMess[i].Text;
                    var tr = trMess[i].Text;
                    if (en.Equals(tr))
                        continue; // Skipping not translated strings

                    /*if (translates.TryGetValue(en, out var tr) && !tr.Equals(ru))
                    {
                        Console.WriteLine($"Multiple tr: {en} {mess.FileName}");
                        Console.WriteLine("====================");
                        Console.WriteLine(tr);
                        Console.WriteLine("====================");
                        Console.WriteLine(ru);
                        Console.WriteLine();
                    }*/

                    translates[en] = tr;
                    translatesSimple[Simple(en)] = tr;
                }
            }

            Console.WriteLine("Translating...");

            foreach (var txt in package2.Texts)
            {
                var enTxt = txt.GetStrings();
                var trTxt = translate2.Get(txt).GetStrings();

                bool hasTranslate = false;

                for (int i = 0; i < enTxt.Length; i++)
                {
                    var en = enTxt[i];

                    if (translates.TryGetValue(en, out var tr))
                    {
                        trTxt[i] = tr;
                        hasTranslate = true;
                    }
                    else
                    {
                        // Found similar
                        if (translatesSimple.TryGetValue(Simple(en), out tr))
                        {
                            trTxt[i] = tr;
                            hasTranslate = true;
                            /*Console.WriteLine($"{txt.FileName}");
                            Console.WriteLine(en);
                            Console.WriteLine("===============");
                            Console.WriteLine(res);
                            Console.WriteLine();*/
                        }
                    }
                }

                if (hasTranslate)
                {
                    txt.SetStrings(trTxt);
                    txt.SavePatch();
                }
            }

            return Task.CompletedTask;
        }

        // Removing all spaces and convert to lowercase for better comparing
        private string Simple(string str)
        {
            int oldLen;
            do
            {
                oldLen = str.Length;
                str = str.Replace("  ", " ");
            }
            while (oldLen != str.Length);
            return str.ToLower();
        }
    }
}
