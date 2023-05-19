using SCI_Lib.Compression;
using SCI_Lib.Decompression;
using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace SCI_Lib.Resources
{
    public class Resource
    {
        private ResourceFileInfo _info;

        public void Init(SCIPackage package, ResType type, ushort number, byte resNum, int offset)
        {
            Package = package;
            Type = type;
            Number = number;

            if (offset >= 0)
            {
                if (type == ResType.Message && package.ExternalMessages)
                    Volumes.Add(new VolumeOffset(resNum, offset, "RESOURCE.MSG"));
                else
                    Volumes.Add(new VolumeOffset(resNum, offset));
            }
        }

        public void Init(SCIPackage package, ResType type, ushort number, byte resNum, ResourceFileInfo info)
        {
            Package = package;
            Type = type;
            Number = number;
            if (type == ResType.Message && package.ExternalMessages)
                Volumes.Add(new VolumeOffset(resNum, -1, "RESOURCE.MSG"));
            else
                Volumes.Add(new VolumeOffset(resNum, -1));
            _info = info;
        }

        public SCIPackage Package { get; private set; }

        public GameEncoding GameEncoding => Package.GameEncoding;

        public ResType Type { get; private set; }

        public ushort Number { get; private set; }

        public class VolumeOffset
        {
            public byte Num { get; }

            public int Offset { get; }

            public string FileName { get; }

            public VolumeOffset(byte num, int offset)
            {
                Num = num;
                Offset = offset;
                FileName = $"RESOURCE.{Num:D3}";
            }

            public VolumeOffset(byte num, int offset, string fileName)
            {
                Num = num;
                Offset = offset;
                FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            }
        }

        public List<VolumeOffset> Volumes { get; } = new List<VolumeOffset>();

        public string FileName => Package.GetResFileName(this);

        public ResourceFileInfo GetInfo() => _info ??= Package.LoadResourceInfo(Volumes[0].FileName, Volumes[0].Offset);

        public override string ToString() => FileName;

        public byte[] GetContent(int volume = 0)
        {
            return ReadContent(Package.GameDirectory, volume);
        }


        private byte[] ReadContent(string dir, int volume = 0)
        {
            var path = Path.Combine(dir, FileName);
            if (File.Exists(path)) // Если есть внешний файл, используем его
            {
                using FileStream fs = File.OpenRead(path);

                fs.Seek(1, SeekOrigin.Current);
                var second = fs.ReadB();
                int offset = GetResourceOffsetInFile(second);
                fs.Seek(offset, SeekOrigin.Current);

                return fs.ReadBytes((int)(fs.Length - fs.Position));
            }

            if (Volumes.Count == 0) return Array.Empty<byte>();

            var vol = Volumes[volume];
            var info = GetInfo();

            using (FileStream fs = File.OpenRead(Path.Combine(dir, vol.FileName)))
            {
                fs.Position = vol.Offset + info.HeadSize;

                if (info.Method == 0) // Uncompressed
                {
                    return fs.ReadBytes(info.DecompSize);
                }
                else
                {
                    Decompressor decomp = info.GetDecompressor();
                    return decomp.Unpack(fs, info.CompSize, info.DecompSize);
                }
            }
        }

        public static int GetResourceOffsetInFile(byte secondHeaderByte)
        {
            // The upper byte appears to indicate where the data starts.
            // Some isolated resource files have random data stuffed at the beginning (e.g. name and such)

            // In SQ5 though, we need to special case:
            if ((secondHeaderByte & 0x80) != 0)
            {
                switch (secondHeaderByte & 0x7f)
                {
                    case 0:
                        return 24;
                    case 1:
                        return 2;
                    case 4:
                        return 8;
                }
            }
            return secondHeaderByte;
        }

        byte[] _compressed = null;

        public byte[] GetCompressed(int volume = 0)
        {
            if (_compressed != null) return _compressed;

            var resOff = Volumes[volume];

            var info = volume == 0 ? GetInfo() : Package.LoadResourceInfo(resOff.FileName, resOff.Offset);

            using FileStream fs = File.OpenRead(Path.Combine(Package.GameDirectory, resOff.FileName));

            fs.Position = resOff.Offset + info.HeadSize;
            return _compressed = fs.ReadBytes(info.CompSize);
        }

        public virtual byte[] GetPatch()
        {
            return GetContent();
        }

        public virtual void SavePatch(string path = null)
        {
            SavePatch(GetPatch(), path);
        }

        public void SavePatch(byte[] data, string path = null)
        {
            if (path == null)
                path = Path.Combine(Package.GameDirectory, FileName.ToLower());

            if (File.Exists(path))
                File.Delete(path);

            using FileStream fs = File.Create(path);
            Save(fs, data);
        }

        public byte[] Export()
        {
            var data = GetContent();

            MemoryStream mem = new MemoryStream();
            Save(mem, data);
            return mem.ToArray();
        }

        protected virtual void WriteHeader(Stream stream)
        {
            stream.WriteByte((byte)Type);
            stream.WriteByte(0);
        }

        public void Save(Stream stream, byte[] data)
        {
            WriteHeader(stream);
            stream.Write(data, 0, data.Length);
        }

        public virtual string[] GetStrings()
        {
            return null;
        }

        public virtual void SetStrings(string[] strings)
        {
            throw new NotImplementedException();
        }

        public void Pack(Stream stream, byte volNum)
        {
            var info = GetInfo();
            var path = Path.Combine(Package.GameDirectory, FileName);

            var begin = stream.Position;
            stream.Seek(info.HeadSize, SeekOrigin.Current);

            var ri = Volumes.FindIndex(r => r.Num == volNum);
            var res = Volumes[ri];

            if (!File.Exists(path)) // Файл не был распакован, считываем запакованный и так же складываем
            {
                var data = GetCompressed();
                stream.Write(data);
            }
            else
            {
                byte[] data = ReadContent(Package.GameDirectory);
                info.DecompSize = (ushort)data.Length;

                if (info.Method != 0)
                {
                    Compressor comp = info.GetCompressor();
                    comp.Pack(data, stream);
                    info.CompSize = (ushort)comp.CompSize;
                    info.DecompSize = (ushort)comp.DecompSize;
                }
                else
                {
                    stream.Write(data);
                    info.CompSize = (ushort)data.Length;
                }
            }

            Volumes[ri] = new VolumeOffset(volNum, (int)begin);

            var end = stream.Position;
            stream.Seek(begin, SeekOrigin.Begin);
            info.Write(stream);
            stream.Seek(end, SeekOrigin.Begin);
        }
    }
}
