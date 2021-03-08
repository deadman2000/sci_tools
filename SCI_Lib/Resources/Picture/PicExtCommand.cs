using SCI_Lib.Utils;
using System;

namespace SCI_Lib.Resources.Picture
{
    abstract class PicExtCommand : PicCommand
    {
        private byte _extcode;

        public PicExtCommand(byte extcode) : base(PicOpCode.OPX)
        {
            _extcode = extcode;
            if (SCIPicture.LOG) Console.WriteLine($"    Ex command: {extcode:X2}");
        }

        public override void Write(ByteBuilder bb)
        {
            bb.AddByte(_extcode);
            WriteExt(bb);
        }

        protected abstract void WriteExt(ByteBuilder bb);
    }
}
