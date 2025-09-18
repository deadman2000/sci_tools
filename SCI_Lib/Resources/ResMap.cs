using SCI_Lib.Resources.Audio;
using SCI_Lib.Utils;
using System;
using System.IO;

namespace SCI_Lib.Resources
{
    public class ResMap : Resource
    {
        private AudioOffset[] _offsets;

        public AudioOffset[] GetOffsets()
        {
            if (_offsets != null) return _offsets;

            var data = GetContent();
            int recordSize = 0;
            for (int i = data.Length - 1; i > 0; i--)
            {
                if (data[i] == 0xff)
                    recordSize++;
                else
                    break;
            }

            int count = data.Length / recordSize;

            _offsets = new AudioOffset[count];
            var ms = new MemoryStream(data);
            int offset = 0;
            for (int i = 0; i < count; i++)
            {
                if (recordSize == 5)
                {
                    var num = ms.ReadUShortBE();
                    offset += ms.Read3ByteBE();
                    _offsets[i] = new(num, offset);
                }
                else
                    throw new NotImplementedException();
            }

            return _offsets;
        }
    }
}
