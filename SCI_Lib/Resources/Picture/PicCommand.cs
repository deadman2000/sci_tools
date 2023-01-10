using SCI_Lib.Utils;
using System;

namespace SCI_Lib.Resources.Picture
{
    class PicCommand
    {
        public PicOpCode OpCode { get; private set; }

        public byte[] Args { get; private set; }

        public PicCommand(PicOpCode opcode)
        {
            OpCode = opcode;

            if (SCIPicture1.LOG) Console.WriteLine($"Code {opcode}");
        }

        public PicCommand(PicOpCode opcode, byte[] args)
        {
            OpCode = opcode;
            Args = args;

            if (SCIPicture1.LOG) Console.WriteLine($"Code {opcode} len: {Args.Length}");
        }

        public virtual void Write(ByteBuilder bb)
        {
            if (Args != null)
                bb.AddBytes(Args);
        }
    }
}
