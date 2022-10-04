using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Pictures;
using SCI_Lib.Resources;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SCI_Tools
{
    // map_font -d D:\Dos\GAMES\DaggerAmonRa\TRANSLATE\ -f 1900 -m D:\Projects\Translate\letters.txt
    // Переносит символы согласно схеме. Схема описывается в файле в формате B=Б в каждой строке
    [Command("map_font", Description = "Export font to images")]
    class MapFont : PackageCommand
    {
        [Option(Description = "Font number", LongName = "font", ShortName = "f")]
        public ushort Font { get; set; }

        [Option(Description = "Map file", LongName = "map", ShortName = "m")]
        public string MapFile { get; set; }

        protected override async Task Do()
        {
            var res = package.GetResource<ResFont>(Font);
            var font = res.GetFont();

            var lines = await File.ReadAllLinesAsync(MapFile);
            var map = lines.Select(l => l.Split('=')).ToArray();

            foreach (var parts in map)
            {
                if (parts.Length != 2) continue;
                char src = parts[0][0];
                char dst = parts[1][0];

                var srcId = Array.IndexOf(package.GameEncoding.AllChars, src);
                var dstId = Array.IndexOf(package.GameEncoding.AllChars, dst);

                if (srcId == -1) throw new Exception($"Letter '{src}' not found in game encoding");
                if (dstId == -1) throw new Exception($"Letter '{dst}' not found in game encoding");

                while (font.Frames.Count <= dstId)
                    font.Frames.Add(new SpriteFrame(1, 1));

                font.Frames[dstId] = new SpriteFrame(font.Frames[srcId]);
                font.Frames[srcId] = new SpriteFrame(1, 1);
            }

            res.SetFont(font);
            res.SavePatch();
        }
    }
}
