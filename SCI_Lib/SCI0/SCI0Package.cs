﻿using SCI_Lib.Resources;
using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
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
            if (!ReadMap(fs, 26))
            {
                if (!ReadMap(fs, 28))
                    throw new FormatException();
            }
        }

        protected bool ReadMap(FileStream fs, int shift)
        {
            Resources.Clear();
            fs.Seek(0, SeekOrigin.Begin);
            HashSet<int> fileChecked = new();

            while (true)
            {
                ushort typeNum = fs.ReadUShortBE();
                uint fnOffset = fs.ReadUIntBE();
                if (typeNum == 0xffff && fnOffset == 0xffffffff) break;

                var type = SCI0_ResMap[typeNum >> 11];
                var num = (ushort)(typeNum & 0x7ff);
                var resNum = (byte)(fnOffset >> shift);
                var offset = (int)(fnOffset & 0x3ffffff);

                var res = AddResource(type, num);
                res.SetupOffset(resNum, offset);

                if (!fileChecked.Contains(resNum))
                {
                    if (!File.Exists(Path.Combine(GameDirectory, $"RESOURCE.{resNum:D3}")))
                        return false;
                    fileChecked.Add(resNum);
                }
            }

            return true;
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
            ResType.View => "VIEW",
            ResType.Picture => "PIC",
            ResType.Script => "SCRIPT",
            ResType.Text => "TEXT",
            ResType.Sound => "SOUND",
            ResType.Vocabulary => "VOCAB",
            ResType.Font => "FONT",
            ResType.Cursor => "CURSOR",
            ResType.Patch => "PATCH",
            ResType.Palette => "PALETTE",
            _ => throw new NotImplementedException(),
        };

        public override bool TryFileNameToRes(string fileName, out ResType type, out ushort number)
        {
            var parts = fileName.Split('.');
            if (parts.Length == 2 && ushort.TryParse(parts[1], out number))
            {
                type = GetResType(parts[0]);
                if (type != ResType.Unknown)
                    return true;
            }

            type = default;
            number = default;
            return false;
        }

        private static ResType GetResType(string name) => name.ToUpper() switch
        {
            "VIEW" => ResType.View,
            "PIC" => ResType.Picture,
            "SCRIPT" => ResType.Script,
            "TEXT" => ResType.Text,
            "SOUND" => ResType.Sound,
            "VOCAB" => ResType.Vocabulary,
            "FONT" => ResType.Font,
            "CURSOR" => ResType.Cursor,
            "PATCH" => ResType.Patch,
            _ => ResType.Unknown,
        };
    }
}
