using McMaster.Extensions.CommandLineUtils;
using NAudio.Wave;
using SCI_Lib.Resources.Audio;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SCI_Tools
{
    [Command("audio_replace", Description = "Replace audio resources")]
    internal class AudioReplace : PackageCommand
    {
        [DirectoryExists]
        [Option(Description = "Source folder", LongName = "source")]
        public string Source { get; set; }

        protected override Task Do()
        {
            using var audio = package.CreateAudioManager();

            List<AudioMap> maps = new();

            foreach (var number in audio.GetMaps())
            {
                var map = audio.ReadMap(number);
                map.ReadHeaders();
                maps.Add(map);
            }

            var wavFiles = Directory.GetFiles(Source, "*.wav");
            foreach (var file in wavFiles)
            {
                var parts = Path.GetFileNameWithoutExtension(file).Split('.');
                var mapNumber = int.Parse(parts[0]);
                var tuple = (uint.Parse(parts[1]) << 24)
                    | (uint.Parse(parts[2]) << 16)
                    | (uint.Parse(parts[3]) << 8)
                    | uint.Parse(parts[4]);
                
                var map = maps.First(m => m.Number == mapNumber);
                map.Samples.RemoveAll(s => s.Number == tuple);

                var size = new FileInfo(file).Length;

                if (size != 0)
                {
                    var waveReader = new WaveFileReader(file);
                    var sol = SOLCodec.Encode(waveReader);
                    var sample = AudioSample.CreateSOL(sol, tuple);
                    map.Samples.Add(sample);
                }
            }

            audio.SaveAll(maps);

            return Task.CompletedTask;
        }
    }
}
