using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace SCI_Lib.Resources.Picture
{
    public class SCIPicture
    {
        private List<PicCommand> commands;

        private PicImage _img;
        private PicPalette _palette;

        public static bool LOG = false;

        public byte[] Image => _img?.Image;

        public SCIPicture(byte[] data)
        {
            commands = new List<PicCommand>();

            using var stream = new MemoryStream(data.Length);
            stream.Write(data, 0, data.Length);
            stream.Seek(0, SeekOrigin.Begin);

            PicOpCode opcode = 0;
            while (true)
            {
                var b = stream.Peek();
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
                        commands.Add(new PicCommand(opcode));
                        break;

                    case PicOpCode.SET_COLOR:
                    case PicOpCode.SET_CONTROL:
                    case PicOpCode.SET_PRIORITY:
                    case PicOpCode.SET_PATTERN:
                        commands.Add(new PicCommand(opcode, stream.ReadBytes(1))); break;

                    case PicOpCode.RELATIVE_MEDIUM_LINES: ReadRelativeMediumLines(stream); break;
                    case PicOpCode.RELATIVE_LONG_LINES: ReadRelativeLongLines(stream); break;
                    case PicOpCode.RELATIVE_SHORT_LINES: ReadRelativeShortLines(stream); break;

                    case PicOpCode.FILL: commands.Add(new PicCommand(opcode, stream.ReadBytes(3))); break;

                    case PicOpCode.OPX: ReadExt(stream); break;

                    case PicOpCode.END:
                        commands.Add(new PicCommand(opcode));
                        return;

                    default:
#if DEBUG
                        Debugger.Break();
#endif
                        throw new FormatException($"Unsupported opcode {opcode}");
                }
            }
        }

        private void ReadRelativeMediumLines(MemoryStream stream)
        {
            var start = stream.Position;

            if (LOG) Console.WriteLine("---- Medium lines");
            var p = stream.ReadPicAbsCoord();
            if (LOG) Console.WriteLine($"{p.X} {p.Y}");

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

                if (LOG) Console.WriteLine($"{x} {y}");
                p.X = x;
                p.Y = y;
            }

            if (LOG) Console.WriteLine();

            var len = stream.Position - start;
            stream.Seek(start, SeekOrigin.Begin);
            commands.Add(new PicCommand(PicOpCode.RELATIVE_MEDIUM_LINES, stream.ReadBytes((int)len)));
        }

        private void ReadRelativeLongLines(MemoryStream stream)
        {
            var start = stream.Position;

            if (LOG) Console.WriteLine("---- Long lines");
            var p = stream.ReadPicAbsCoord();
            if (LOG) Console.WriteLine($"{p.X} {p.Y}");

            while (stream.Peek() < 0xf0)
            {
                p = stream.ReadPicAbsCoord();
                if (LOG) Console.WriteLine($"{p.X} {p.Y}");
            }

            if (LOG) Console.WriteLine();

            var len = stream.Position - start;
            stream.Seek(start, SeekOrigin.Begin);
            commands.Add(new PicCommand(PicOpCode.RELATIVE_LONG_LINES, stream.ReadBytes((int)len)));
        }

        private void ReadRelativeShortLines(MemoryStream stream)
        {
            var start = stream.Position;

            if (LOG) Console.WriteLine("-- Short lines");
            var p = stream.ReadPicAbsCoord();
            if (LOG) Console.WriteLine($"{p.X} {p.Y}");

            while (stream.Peek() < 0xf0)
            {
                p = stream.ReadPicRelCoord(p);
                if (LOG) Console.WriteLine($"{p.X} {p.Y}");
            }

            if (LOG) Console.WriteLine();

            var len = stream.Position - start;
            stream.Seek(start, SeekOrigin.Begin);
            commands.Add(new PicCommand(PicOpCode.RELATIVE_SHORT_LINES, stream.ReadBytes((int)len)));
        }

        private void ReadExt(MemoryStream stream)
        {
            var extcode = stream.ReadB();
            switch (extcode)
            {
                case 0x01: commands.Add(_img = new PicImage(stream)); break;
                case 0x02: commands.Add(_palette = new PicPalette(stream)); break;
                case 0x04: commands.Add(new PicPriorityBars(stream)); break;
                default: throw new FormatException($"Unknown extcode {extcode:X2}");
            }
        }

        public Image GetBackground()
        {
            if (_img == null) return null;

            var bmp = new Bitmap(_img.Width, _img.Height, PixelFormat.Format8bppIndexed);
            var pal = bmp.Palette;
            for (int i = 0; i < 256; i++)
                pal.Entries[i] = _palette.Colors[i].GetColor();

            bmp.Palette = pal;

            var data = bmp.LockBits(new Rectangle(0, 0, _img.Width, _img.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            IntPtr scan0 = data.Scan0;
            Marshal.Copy(_img.Image, 0, scan0, _img.Image.Length);

            bmp.UnlockBits(data);

            return bmp;
        }

        public void SetBackground(Bitmap bmp)
        {
            if (bmp.Width != _img.Width || bmp.Height != _img.Height)
                throw new ArgumentException("Different image size");

            if (bmp.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new ArgumentException($"Wrong image pixel format {bmp.PixelFormat}");

            var data = bmp.LockBits(new Rectangle(0, 0, _img.Width, _img.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            var scan0 = data.Scan0;
            Marshal.Copy(scan0, _img.Image, 0, _img.Image.Length);

            bmp.UnlockBits(data);
        }

        public byte[] GetBytes()
        {
            PicOpCode prev = 0;

            ByteBuilder bb = new ByteBuilder();
            foreach (var cmd in commands)
            {
                if (cmd.OpCode != prev || cmd.OpCode == PicOpCode.OPX)
                    bb.AddByte((byte)cmd.OpCode);

                cmd.Write(bb);

                prev = cmd.OpCode;
            }

            return bb.GetArray();
        }
    }
}
