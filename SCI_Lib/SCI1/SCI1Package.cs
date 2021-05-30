using SCI_Lib.Resources;
using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SCI_Lib.SCI1
{
    public class SCI1Package : SCIPackage
    {
        public SCI1Package(string directory, Encoding enc)
            : base(directory, enc)
        {
        }

        class ResMapOffset
        {
            public ResMapOffset(ResType type, ushort offset)
            {
                Type = type;
                Offset = offset;
            }

            public ResType Type { get; set; }

            public ushort Offset { get; set; }

            public int RecordsCount { get; set; }
        }

        bool? _isSCI_11;


        // https://wiki.scummvm.org/index.php?title=SCI/Specifications/Resource_files/SCI1_resources
        // http://sci.sierrahelp.com/Documentation/SCISpecifications/09-SCI1Resources.html
        protected override void ReadMap(FileStream fs)
        {
            if (!_isSCI_11.HasValue)
                DetectVersion(fs);

            fs.Position = 0;

            List<ResMapOffset> offsets = new List<ResMapOffset>();
            int entriesSize = _isSCI_11.Value ? 5 : 6;

            ResMapOffset off = null;
            while (true)
            {
                int t = fs.ReadByte();
                if ((t < 0x80 || t > 0x91) && t != 0xff)
                    throw new FormatException("Wrong format");

                ResType type = (ResType)t;
                ushort offset = fs.ReadUShortBE();
                if (off != null) off.RecordsCount = (offset - off.Offset) / entriesSize; // Высчитываем количество записей исходя из границ блоков

                if (type == ResType.End) break;

                off = new ResMapOffset(type, offset);

                offsets.Add(off);
            }

            for (int i = 0; i < offsets.Count; i++)
            {
                fs.Position = offsets[i].Offset;
                for (int j = 0; j < offsets[i].RecordsCount; j++)
                {
                    ushort num = fs.ReadUShortBE();
                    int offset;
                    byte resNum = 0;

                    if (_isSCI_11.Value)
                    {
                        offset = fs.ReadUShortBE();
                        offset |= fs.ReadByte() << 16;
                        offset <<= 1;
                    }
                    else
                    {
                        int address = fs.ReadIntBE();
                        resNum = (byte)((address >> 28) & 0x0F);
                        offset = address & 0x0FFFFFFF;
                    }

                    //Console.WriteLine($"{num,6} {offset:X08} {offsets[i].Type} {resNum}");

                    var ex = Resources.Find(r => r.Type == offsets[i].Type && r.Number == num);
                    if (ex != null)
                    {
                        ex.Volumes.Add(new Resource.VolumeOffset(resNum, offset));
                    }
                    else
                    {
                        Resource res = CreateRes(offsets[i].Type);
                        res.Init(this, offsets[i].Type, num, resNum, offset);
                        Resources.Add(res);
                    }
                }
            }

            var allFiles = Directory.GetFiles(GameDirectory).Select(f => Path.GetFileName(f)).Select(f => f.ToUpper());
            foreach (ResType rt in Enum.GetValues(typeof(ResType)))
            {
                if ((byte)rt < 0x80 || (byte)rt > 0x91)
                    continue;

                var ext = GetExtension(rt);

                foreach (var f in allFiles.Where(f => f.EndsWith("." + ext)))
                {
                    if (!ushort.TryParse(Path.GetFileNameWithoutExtension(f), out var num))
                        continue;

                    if (GetResource(f) == null)
                    {
                        ushort method = 0;
                        byte resNum = 0;

                        var first = Resources.FirstOrDefault(r => r.Type == rt);
                        if (first != null)
                        {
                            method = first.GetInfo().Method;
                            resNum = first.Volumes[0].Num;
                        }

                        var info = new ResourceFileInfo1((byte)rt, num, method);
                        var res = CreateRes(rt);
                        res.Init(this, rt, num, resNum, info);
                        Resources.Add(res);
                    }
                }
            }
        }

        private void DetectVersion(FileStream fs)
        {
            int t = fs.ReadByte();
            if ((t < 0x80 || t > 0x91) && t != 0xff)
                throw new FormatException("Wrong format");

            var offset = fs.ReadUShortBE();
            fs.Position += 1;
            var next = fs.ReadUShortBE();
            var bytes = next - offset;

            fs.Position = offset;
            if (bytes % 5 == 0 && CheckMap(fs, 3, bytes / 5))
            {
                _isSCI_11 = true;
                return;
            }

            fs.Position = offset;
            if (bytes % 6 == 0 && CheckMap(fs, 4, bytes / 6))
            {
                _isSCI_11 = false;
                return;
            }

            throw new FormatException("Unknown format");
        }

        private static bool CheckMap(FileStream fs, int step, int count)
        {
            try
            {
                ushort prev, curr;
                prev = fs.ReadUShortBE();
                count--;

                while (count > 0)
                {
                    fs.Position += step;
                    curr = fs.ReadUShortBE();
                    count--;

                    if (curr < prev)
                        return false;

                    prev = curr;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override void SaveMap(FileStream fs)
        {
            var byType = Resources.GroupBy(r => r.Type);

            fs.Seek((byType.Count() + 1) * 3, SeekOrigin.Begin);

            ushort offset;
            int i = 0;
            foreach (var gr in byType)
            {
                offset = (ushort)fs.Position;

                foreach (var r in gr)
                {
                    foreach (var resOffset in r.Volumes)
                    {
                        fs.WriteUShortBE(r.Number);
                        fs.WriteUIntBE((uint)(resOffset.Offset | (resOffset.Num << 28)));
                    }
                }

                var pos = fs.Position;

                fs.Seek(i * 3, SeekOrigin.Begin);

                fs.WriteByte((byte)gr.Key);
                fs.WriteUShortBE(offset);

                fs.Seek(pos, SeekOrigin.Begin);
                i++;
            }
            offset = (ushort)fs.Position;

            fs.Seek(i * 3, SeekOrigin.Begin);
            fs.WriteByte(0xff);
            fs.WriteUShortBE(offset);
        }

        public override ResourceFileInfo LoadResourceInfo(string resourceFileName, int offset)
        {
            return new ResourceFileInfo1(Path.Combine(GameDirectory, resourceFileName), offset);
        }

        public override string GetResFileName(Resource resource) => $"{resource.Number}.{GetExtension(resource.Type)}";

        private static string GetExtension(ResType type)
        {
            return type switch
            {
                ResType.View => "V56",
                ResType.Picture => "P56",
                ResType.Script => "SCR",
                ResType.Text => "TEX",
                ResType.Sound => "SND",
                ResType.Vocabulary => "VOC",
                ResType.Font => "FON",
                ResType.Cursor => "CUR",
                ResType.AudioPath => "PAT",
                ResType.Bitmap => "BIT",
                ResType.Palette => "PAL",
                ResType.CDAudio => "CDA",
                ResType.Audio => "AUD",
                ResType.Sync => "SYN",
                ResType.Message => "MSG",
                ResType.Map => "MAP",
                ResType.Heap => "HEP",
                //ResType.Patch => "PAT",
                _ => throw new NotImplementedException(),
            };
        }

    }
}
