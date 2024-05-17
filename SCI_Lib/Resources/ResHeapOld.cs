using SCI_Lib.Utils;
using System;
using System.IO;

namespace SCI_Lib.Resources
{
    [Obsolete]
    public class ResHeapOld : Resource
    {
        private byte[] _data;
        private ushort[] _strProps;
        private ushort[] _strAddr;
        private string[] _strings;

        public override string[] GetStrings()
        {
            if (_strings == null) Read();
            return _strings;
        }

        public override void SetStrings(string[] strings)
        {
            _strings = strings;
        }

        public void Read()
        {
            _data = GetContent();
            using var ms = new MemoryStream(_data);
            var strOffset = ms.ReadUShortBE();

            // Read strings
            ms.Position = strOffset;
            var count = ms.ReadUShortBE();

            _strProps = new ushort[count];
            _strAddr = new ushort[count];
            _strings = new string[count];

            for (int i = 0; i < count; i++)
                _strProps[i] = ms.ReadUShortBE();

            for (int i = 0; i < _strProps.Length; i++)
            {
                ms.Position = _strProps[i];
                var addr = ms.ReadUShortBE();
                _strAddr[i] = addr;

                ms.Position = addr;
                _strings[i] = ms.ReadString(GameEncoding);
            }
        }

        public override byte[] GetPatch()
        {
            if (_data == null) return GetContent();

            using var ms = new MemoryStream();
            ms.WriteUShortBE(0);
            ms.Write(_data, 2, _strAddr[0] - 2);

            var newAddr = new ushort[_strings.Length];
            for (int i = 0; i < _strings.Length; i++)
            {
                newAddr[i] = (ushort)ms.Position;
                var bytes = GameEncoding.GetBytes(_strings[i]);
                ms.Write(bytes);
                ms.WriteByte(0);
            }
            if (ms.Position % 2 == 1) ms.WriteByte(0);

            // Strings map
            var mapAddr = (ushort)ms.Position;
            ms.WriteUShortBE((ushort)_strings.Length);
            for (int i = 0; i < _strProps.Length; i++)
                ms.WriteUShortBE(_strProps[i]);

            // Replace address map
            ms.Position = 0;
            ms.WriteUShortBE(mapAddr);

            // Replace string addresses
            for (int i = 0; i < _strProps.Length; i++)
            {
                ms.Position = _strProps[i];
                ms.WriteUShortBE(newAddr[i]);
            }

            return ms.ToArray();
        }
    }
}
