using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCI_Lib.Resources.Audio
{
    public class AudioManager : IDisposable
    {
        private readonly SCIPackage _package;
        private readonly string _audDir;
        private readonly string _audPath;
        private readonly string _sfxPath;
        private Stream _audStream = null;
        private Stream _sfxStream = null;

        public AudioManager(SCIPackage package)
        {
            _package = package;

            _audDir = Path.Combine(package.GameDirectory, "AUDIO");
            _audPath = Path.Combine(_audDir, "RESOURCE.AUD");
            _sfxPath = Path.Combine(package.GameDirectory, "RESOURCE.SFX");
        }

        public string AudPath => _audPath;

        public AudioSample[] ReadSFX()
        {
            _sfxStream ??= File.OpenRead(_sfxPath);
            var res = _package.GetResource<ResMap>(65535);
            var offsets = res.GetOffsets();
            return offsets
                .Select(o => new AudioSample(_sfxStream, o.Offset, o.Number))
                .ToArray();
        }

        public int[] GetMaps()
        {
            var maps = Directory.GetFiles(_audDir, "*.MAP");
            return maps.Select(p => Path.GetFileNameWithoutExtension(p))
                .Select(n => int.Parse(n))
                .OrderBy(i => i)
                .ToArray();
        }

        public AudioMap ReadMap(int mapNumber)
        {
            _audStream ??= File.OpenRead(_audPath);

            AudioMap map = new(mapNumber, _audDir);
            map.Read(_audStream);
            return map;
        }

        public void Dispose()
        {
            CloseStreams();
        }

        public void CloseStreams()
        {
            _audStream?.Dispose();
            _sfxStream?.Dispose();
            _audStream = null;
            _sfxStream = null;
        }

        public void SaveAll(IEnumerable<AudioMap> maps)
        {
            var tempFilePath = Path.GetTempFileName();
            try
            {
                using (var outAud = File.OpenWrite(tempFilePath))
                {
                    foreach (var map in maps)
                    {
                        map.Write(outAud);
                    }

                    outAud.Flush();
                }

                CloseStreams();
                File.Delete(_audPath);
                File.Move(tempFilePath, _audPath);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }

            foreach (var map in maps)
            {
                map.SaveMap();
            }
        }
    }
}
