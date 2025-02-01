using SCI_Lib.Resources;
using SCI_Lib.Utils;
using System;
using System.IO;
using System.Text;

namespace SCI_Lib.SCI0
{
    public class SCI0Package : SCIPackage
    {
        public SCI0Package(string directory, Encoding enc)
            : base(directory, enc)
        {
        }

        static readonly ResType[] SCI0_ResMap = new ResType[] {
            ResType.View, ResType.Picture, ResType.Script, ResType.Text,      // 0x00-0x03
            ResType.Sound, ResType.Memory, ResType.Vocabulary, ResType.Font,  // 0x04-0x07
            ResType.Cursor, ResType.Patch, ResType.Bitmap, ResType.Palette,   // 0x08-0x0B
            ResType.CDAudio, ResType.Audio, ResType.Sync, ResType.Message,    // 0x0C-0x0F
            ResType.Map, ResType.Heap, ResType.Audio36, ResType.Sync36,       // 0x10-0x13
            ResType.Translation, ResType.Rave                                 // 0x14-0x15
        };

        // https://wiki.scummvm.org/index.php?title=SCI/Specifications/Resource_files/SCI0_resources
        // http://sci.sierrahelp.com/Documentation/SCISpecifications/08-ResourceFiles.html
        // https://github.com/scummvm/scummvm/blob/master/engines/sci/resource.cpp#L1779
        protected override void ReadMap(FileStream fs)
        {
            while (true)
            {
                ushort typeNum = fs.ReadUShortBE();
                uint fnOffset = fs.ReadUIntBE();
                if (typeNum == 0xffff && fnOffset == 0xffffffff) break;

                var type = SCI0_ResMap[typeNum >> 11];
                var num = (ushort)(typeNum & 0x7ff);
                var resNum = (byte)(fnOffset >> 26);
                var offset = (int)(fnOffset & 0x3ffffff);

                var res = CreateResource(type, num);
                res.Init(this, type, num, resNum, offset);
                Resources.Add(res);
            }
        }

        protected override void SaveMap(FileStream fs)
        {
            throw new NotImplementedException();
        }

        public override ResourceFileInfo LoadResourceInfo(string resourceFileName, int offset)
        {
            return new ResourceFileInfo0(Path.Combine(GameDirectory, resourceFileName), offset);
        }

        public override string GetResFileName(ResType type, int number) => $"{GetResName(type)}.{number:D3}";

        private static string GetResName(ResType type) => type switch
        {
            ResType.View => "view",
            ResType.Picture => "pic",
            ResType.Script => "script",
            ResType.Text => "text",
            ResType.Sound => "sound",
            ResType.Vocabulary => "vocab",
            ResType.Font => "font",
            ResType.Cursor => "cursor",
            ResType.Patch => "patch",
            ResType.Palette => "palette",
            _ => throw new NotImplementedException(),
        };

        public override (ResType type, int number) FileNameToRes(string fileName)
        {
            var parts = fileName.Split('.');
            if (parts.Length != 2) throw new FormatException($"Invalid file name '{fileName}'");
            if (!int.TryParse(parts[1], out var num)) throw new FormatException($"Invalid file name '{fileName}'");

            return (GetResType(parts[0]), num);
        }

        private static ResType GetResType(string name) => name.ToLower() switch
        {
            "view" => ResType.View,
            "pic" => ResType.Picture,
            "script" => ResType.Script,
            "text" => ResType.Text,
            "sound" => ResType.Sound,
            "vocab" => ResType.Vocabulary,
            "font" => ResType.Font,
            "cursor" => ResType.Cursor,
            "patch" => ResType.Patch,
            _ => throw new NotImplementedException(),
        };
    }
}
