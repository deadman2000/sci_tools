using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SCI_Lib.Resources.Picture
{
    public class PicVector
    {
        public static bool LOG_READ = false;
        public static bool LOG_WRITE = false;

        private readonly List<PicCommand> _commands = new();
        private PicImage _img;
        private PicPalette _palette;

        public PicImage Image => _img;

        public PicPalette Palette => _palette;

        public static PicVector Read(MemoryStream stream)
        {
            PicVector v = new();
            v.ReadStream(stream);
            return v;
        }

        private void ReadStream(MemoryStream stream)
        {
            PicOpCode opcode = 0;
            while (true)
            {
                var b = stream.Peek();
                if (LOG_READ) Console.WriteLine($"{stream.Position:X} Read op code {b:X2}");
                if (b >= 0xf0)
                {
                    opcode = (PicOpCode)b;
                    stream.Seek(1, SeekOrigin.Current);
                }

                switch (opcode)
                {
                    case PicOpCode.DISABLE_VISUAL:
                    case PicOpCode.DISABLE_PRIORITY:
                    case PicOpCode.DISABLE_CONTROL:
                        _commands.Add(new PicCommand(opcode));
                        break;

                    case PicOpCode.SET_COLOR:
                    case PicOpCode.SET_CONTROL:
                    case PicOpCode.SET_PRIORITY:
                    case PicOpCode.SET_PATTERN:
                        _commands.Add(new PicCommand(opcode, stream.ReadBytes(1))); break;

                    case PicOpCode.RELATIVE_MEDIUM_LINES: ReadRelativeMediumLines(stream); break;
                    case PicOpCode.RELATIVE_LONG_LINES: ReadRelativeLongLines(stream); break;
                    case PicOpCode.RELATIVE_SHORT_LINES: ReadRelativeShortLines(stream); break;

                    case PicOpCode.FILL: _commands.Add(new PicCommand(opcode, stream.ReadBytes(3))); break;

                    case PicOpCode.OPX: ReadExt(stream); break;

                    case PicOpCode.END:
                        _commands.Add(new PicCommand(opcode));
                        return;

                    default:
#if DEBUG
                        Debugger.Break();
#endif
                        throw new FormatException($"Unsupported opcode {opcode}");
                }
            }
        }

        public byte[] GetBytes()
        {
            ByteBuilder bb = new();
            Write(bb);
            return bb.GetArray();
        }

        public void Write(ByteBuilder bb)
        {
            foreach (var cmd in _commands)
            {
                if (LOG_WRITE) Console.WriteLine($"{bb.Position:X} Write op code {(byte)cmd.OpCode:X2}");
                bb.AddByte((byte)cmd.OpCode);
                cmd.Write(bb);
            }
        }

        private void ReadRelativeMediumLines(MemoryStream stream)
        {
            var start = stream.Position;

            if (LOG_READ) Console.WriteLine("---- Medium lines");
            var p = stream.ReadPicAbsCoord();
            if (LOG_READ) Console.WriteLine($"  {p.X} {p.Y}");

            while (stream.Peek() < 0xf0)
            {
                var t = stream.ReadB();
                ushort x, y;
                if ((t & 0x80) == 0x80)
                {
                    y = (ushort)(p.Y - (t & 0x7f));
                }
                else
                {
                    y = (ushort)(p.Y + t);
                }

                t = stream.ReadB();
                x = (ushort)(p.X + (sbyte)t);

                if (LOG_READ) Console.WriteLine($"  {x} {y}");
                p.X = x;
                p.Y = y;
            }

            if (LOG_READ) Console.WriteLine();

            var len = stream.Position - start;
            stream.Seek(start, SeekOrigin.Begin);
            _commands.Add(new PicCommand(PicOpCode.RELATIVE_MEDIUM_LINES, stream.ReadBytes((int)len)));
        }

        private void ReadRelativeLongLines(MemoryStream stream)
        {
            var start = stream.Position;

            if (LOG_READ) Console.WriteLine("---- Long lines");
            var p = stream.ReadPicAbsCoord();
            if (LOG_READ) Console.WriteLine($"  {p.X} {p.Y}");

            while (stream.Peek() < 0xf0)
            {
                p = stream.ReadPicAbsCoord();
                if (LOG_READ) Console.WriteLine($"  {p.X} {p.Y}");
            }

            if (LOG_READ) Console.WriteLine();

            var len = stream.Position - start;
            stream.Seek(start, SeekOrigin.Begin);
            _commands.Add(new PicCommand(PicOpCode.RELATIVE_LONG_LINES, stream.ReadBytes((int)len)));
        }

        private void ReadRelativeShortLines(MemoryStream stream)
        {
            var start = stream.Position;

            if (LOG_READ) Console.WriteLine("-- Short lines");
            var p = stream.ReadPicAbsCoord();
            if (LOG_READ) Console.WriteLine($"  {p.X} {p.Y}");

            while (stream.Peek() < 0xf0)
            {
                p = stream.ReadPicRelCoord(p);
                if (LOG_READ) Console.WriteLine($"  {p.X} {p.Y}");
            }

            if (LOG_READ) Console.WriteLine();

            var len = stream.Position - start;
            stream.Seek(start, SeekOrigin.Begin);
            _commands.Add(new PicCommand(PicOpCode.RELATIVE_SHORT_LINES, stream.ReadBytes((int)len)));
        }

        private void ReadExt(MemoryStream stream)
        {
            var extcode = stream.ReadB();
            switch (extcode)
            {
                case 0x01: _commands.Add(_img = new PicImage(stream)); break;
                case 0x02: _commands.Add(_palette = new PicPalette(stream)); break;
                case 0x04: _commands.Add(new PicPriorityBars(stream)); break;
                default: throw new FormatException($"Unknown extcode {extcode:X2}");
            }
        }

    }
}
