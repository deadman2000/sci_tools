using SCI_Lib.Utils;
using System.Collections.Generic;
using System.IO;

namespace SCI_Lib.Resources.Audio
{
    public class AudioMap
    {
        private readonly string _mapPath;

        public AudioMap(SCIPackage package, int number, string folder)
        {
            Package = package;
            Number = number;
            _mapPath = Path.Combine(folder, $"{number}.MAP");
        }

        public SCIPackage Package { get; }

        public int Number { get; }

        public List<AudioSample> Samples { get; set; }

        protected byte[] ReadMap()
        {
            if (File.Exists(_mapPath))
            {
                return File.ReadAllBytes(_mapPath);
            }

            return Package.GetResource<ResMap>((ushort)Number).GetContent();
        }

        public void Read(Stream audStream)
        {
            var data = ReadMap();

            int recordSize = 0;
            for (int i = data.Length - 1; i > 0; i--)
            {
                if (data[i] == 0xff)
                    recordSize++;
                else
                    break;
            }

            Samples = new();
            MemoryStream ms = new(data);
            if (data[0] == 0x90)
            {
                ms.Seek(2, SeekOrigin.Begin);
            }
            var offset = ms.ReadIntBE();
            while (true)
            {
                var n = ms.ReadUIntLE();
                if (n == 0xffffffff) break;
                offset += ms.Read3ByteBE();
                Samples.Add(new(audStream, offset, n));
            }
        }

        public void Write(Stream audStream)
        {
            foreach (var sample in Samples)
            {
                var newOffset = audStream.Position;
                var data = sample.GetRaw();
                audStream.Write(data);
                sample.Offset = newOffset;
            }
        }

        public void ReadHeaders()
        {
            foreach (var sample in Samples)
            {
                sample.ReadHeader();
            }
        }

        public void SaveMap()
        {
            using var file = File.OpenWrite(_mapPath);
            file.WriteUShortLE(0x9000);
            file.WriteUIntBE((uint)Samples[0].Offset);
            var lastOffset = Samples[0].Offset;
            foreach (var sample in Samples)
            {
                file.WriteUIntLE(sample.Number);
                file.Write3ByteBE((int)(sample.Offset - lastOffset));
                lastOffset = sample.Offset;
            }

            for (int i = 0; i < 11; i++)
            {
                file.WriteByte(0xff);
            }
        }
    }
}
