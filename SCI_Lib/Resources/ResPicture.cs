﻿using SCI_Lib.Resources.Picture;
using SCI_Lib.SCI0;
using System.IO;

namespace SCI_Lib.Resources
{
    public class ResPicture : Resource
    {
        private SCIPicture _pic;

        public SCIPicture GetPicture()
        {
            if (_pic != null) return _pic;
            var data = GetContent();
            if (data[0] == 0x26)
            {
                return _pic = new SCIPicture11(Package, data);
            }


            return _pic = new SCIPicture1(data);
        }

        public void SetPicture(SCIPicture pic)
        {
            _pic = pic;
        }

        protected override void WriteHeader(Stream stream)
        {
            if (Package is SCI0Package)
            {
                base.WriteHeader(stream);
            }
            else
            {
                var header = new byte[26];
                header[0] = (byte)Type;
                stream.Write(header);
            }
        }

        /*public override byte[] GetPatch()
        {
            return _pic.GetBytes();
        }*/
    }
}
