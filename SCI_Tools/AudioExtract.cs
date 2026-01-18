using Elasticsearch.Net;
using McMaster.Extensions.CommandLineUtils;
using NAudio.Wave;
using SCI_Lib.Resources.Audio;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SCI_Tools
{
    [Command("audio_extract", Description = "Extract audio resources to wav")]
    internal class AudioExtract : PackageCommand
    {
        private AudioManager _audio;

        [DirectoryExists]
        [Option(Description = "Output folder", LongName = "output")]
        public string Output { get; set; }

        [Option(Description = "Resource ('sfx' or number of map)", LongName = "resource")]
        public string Resource { get; set; }

        public int? Tuple { get; set; }

        [Option(Description = "Play audio", LongName = "play")]
        public bool Play { get; set; }

        protected override Task Do()
        {
            foreach (var file in Directory.GetFiles(Output))
                File.Delete(file);

            _audio = package.CreateAudioManager();

            if (Resource == null)
            {
                ExtractMaps();
            }
            else if (Resource == "sfx")
            {
                ExtractSFX();
            }
            else if (Resource == "aud")
            {
                var maps = _audio.GetMaps();
                foreach (var number in maps)
                {
                    ExtractMap(number);
                }
            }
            else if (int.TryParse(Resource, out var number))
            {
                if (Tuple.HasValue)
                {
                    ExtractMap(number, Tuple.Value);
                }
                else
                {
                    ExtractMap(number);
                }
            }

            return Task.CompletedTask;
        }

        private void ExtractSFX()
        {
            var samples = _audio.ReadSFX();
            foreach (var sample in samples)
            {
                Console.WriteLine($"Extract {sample.Number}...");
                ExtractAudio(sample, $"sfx.{sample.Number}");
            }
        }

        private void ExtractMaps()
        {
            _audio = package.CreateAudioManager();
            foreach (var number in _audio.GetMaps())
            {
                var map = _audio.ReadMap(number);
                foreach (var sample in map.Samples)
                {
                    Console.WriteLine($"Extract {number}.MAP {sample.Number}...");
                    ExtractAudio(sample, $"{number}.{sample.Message}");
                }
            }
        }

        private void ExtractMap(int number)
        {
            _audio = package.CreateAudioManager();
            var map = _audio.ReadMap(number);
            foreach (var sample in map.Samples)
            {
                Console.WriteLine($"Extract {number}.MAP {sample.Number}...");
                ExtractAudio(sample, $"{number}.{sample.Message}");
            }
        }

        private void ExtractMap(int number, int tuple)
        {
            _audio = package.CreateAudioManager();
            var map = _audio.ReadMap(number);
            var sample = map.Samples.First(o => o.Number == tuple);
            ExtractAudio(sample, $"{number}.{sample.Message}");
        }

        private WaveStream GetWave(AudioSample audio)
        {
            audio.ReadHeader();

            if (audio.Format == AudioFormat.SOL)
            {
                var data = audio.ExtractSOL();
                return new RawSourceWaveStream(data, 0, data.Length, new WaveFormat(audio.Rate, 16, 1));
            }

            if (audio.Format == AudioFormat.RIFF)
            {
                var data = audio.GetRaw();
                var ms = new MemoryStream(data);
                return new WaveFileReader(ms);
            }

            throw new NotImplementedException();
        }

        void ExtractAudio(AudioSample audio, string name)
        {
            using var wave = GetWave(audio);

            if (!string.IsNullOrEmpty(Output))
            {
                var fileName = $"{name}.wav";
                var path = Path.Combine(Output, fileName);
                if (File.Exists(path)) File.Delete(path);

                Console.WriteLine($"Saving {fileName}");
                WaveFileWriter.CreateWaveFile(path, wave);
            }

            if (Play)
            {
                Console.WriteLine($"Playing {name}...");
                PlayFile(wave);
                Console.WriteLine("Complete");
            }
        }

        private void PlayFile(IWaveProvider wave)
        {
            using var outputDevice = new WaveOutEvent();
            outputDevice.Init(wave);
            outputDevice.Play();
            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(100);
            }
        }
    }
}
