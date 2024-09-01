using SCI_Lib.Utils;
using System;

namespace SCI_Lib.Resources.Picture
{
    public class PicCommand
    {
        public PicOpCode OpCode { get; private set; }

        public byte[] Args { get; private set; }

        public PicCommand(PicOpCode opcode)
        {
            OpCode = opcode;

            if (PicVector.LOG_READ) Console.WriteLine($"Code {opcode}");
        }

        public PicCommand(PicOpCode opcode, byte[] args)
        {
            OpCode = opcode;
            Args = args;

            if (PicVector.LOG_READ) Console.WriteLine($"Code {opcode} len: {Args.Length}\n  {Helpers.ByteToHex(Args)}");
        }

        public virtual void Write(ByteBuilder bb)
        {
            if (Args != null)
            {
                bb.AddBytes(Args);
                if (PicVector.LOG_WRITE) Console.WriteLine($"  Command {OpCode} len: {Args.Length}\n  {Helpers.ByteToHex(Args)}");
            }
            else
            {
                Console.WriteLine($"  Command {OpCode}");
            }
        }

        public override string ToString() => OpCode.ToString();
    }
}
