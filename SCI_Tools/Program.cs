﻿using McMaster.Extensions.CommandLineUtils;
using System;

namespace SCI_Tools
{
    [Subcommand(
        typeof(CompareTranslates),
        typeof(Test),
        typeof(TranslatedToText),
        typeof(ElasticExport),
        typeof(FontExport),
        typeof(ReplacePic),
        typeof(ExtractPic),
        typeof(ExtractView),
        typeof(Pack),
        typeof(MapFont),
        typeof(PatchEQ),
        typeof(PatchEQCD),
        typeof(PatchLongbow),
        typeof(PatchLarry5),
        typeof(PatchLarry2),
        typeof(PatchLarry3),
        typeof(PatchLarry3PnC)
    )]
    partial class Program
    {
        // https://natemcmaster.github.io/CommandLineUtils/

        public static int Main(string[] args)
            => CommandLineApplication.Execute<Program>(args);

        private int OnExecute(CommandLineApplication app)
        {
            Console.WriteLine("You must specify at a subcommand.");
            app.ShowHelp();
            return 1;
        }
    }
}
