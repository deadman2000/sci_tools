using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCI_Lib.Resources.View
{
    public class SCIView
    {
        private ushort _mask;
        private ushort _unknown2;
        private byte _scaleFlags;
        private byte _scaleRes;
        private ushort _nativeResolution;
        private byte _unknown1;

        public SCIPackage Package { get; }

        public Palette Palette { get; set; }

        public List<Loop> Loops { get; private set; }

        public List<string> BoneNames { get; set; }

        public SCIView(SCIPackage package)
        {
            Package = package;
        }

        public void ReadVGA(byte[] data)
        {
            MemoryStream ms = new(data);
            var loopsCount = ms.ReadB();
            var const80 = ms.ReadB();
            if (const80 != 0x80) throw new FormatException();
            _mask = ms.ReadUShortBE();
            _unknown2 = ms.ReadUShortBE();
            var palOffset = ms.ReadUShortBE();

            ushort[] loopOffsets = new ushort[loopsCount];
            for (int i = 0; i < loopsCount; i++)
                loopOffsets[i] = ms.ReadUShortBE();

            Loops = new List<Loop>(loopsCount);
            for (int i = 0; i < loopsCount; i++)
            {
                var loop = new Loop();
                Loops.Add(loop);

                ms.Position = loopOffsets[i];
                var cellsCount = ms.ReadUShortBE();
                ms.Position += 2; // Skip unknown
                ushort[] cellOffsets = new ushort[cellsCount];
                for (int j = 0; j < cellsCount; j++)
                    cellOffsets[j] = ms.ReadUShortBE();

                for (int j = 0; j < cellsCount; j++)
                {
                    ms.Position = cellOffsets[j];
                    var cell = new Cell(Package, Palette);
                    cell.ReadEVGA(ms, true, 0);
                    loop.Cells.Add(cell);
                }
            }

            ms.Position = palOffset;
            Palette = Palette.Read(ms);
        }

        public byte[] GetBytesVGA()
        {
            ByteBuilder bb = new();

            if (Loops.Count > 255) throw new Exception("Too many loops");
            bb.AddByte((byte)Loops.Count);
            bb.AddByte(0x80);
            bb.AddUShortBE(_mask);
            bb.AddUShortBE(_unknown2);

            var palAddr = bb.Position;
            bb.AddUShortBE(0);

            for (int i = 0; i < Loops.Count; i++)
                bb.AddUShortBE(0);

            for (int i = 0; i < Loops.Count; i++)
            {
                var loop = Loops[i];
                bb.SetUShortBE(palAddr + 2 + i * 2, (ushort)bb.Position);

                bb.AddUShortBE((ushort)loop.Cells.Count);
                bb.AddUShortBE(0);

                var cellAddr = bb.Position;
                for (int j = 0; j < loop.Cells.Count; j++)
                    bb.AddUShortBE(0);

                for (int j = 0; j < loop.Cells.Count; j++)
                {
                    var cell = loop.Cells[j];
                    bb.SetUShortBE(cellAddr + j * 2, (ushort)bb.Position);
                    cell.WriteEVGA(bb, true);
                }
            }

            bb.SetUShortBE(palAddr, (ushort)bb.Position);
            Palette.Write(bb);

            return bb.GetArray();
        }

        public void ReadVGA11(byte[] data)
        {
            MemoryStream ms = new(data);
            var headerSize = ms.ReadUShortBE();
            if (headerSize < 14) throw new FormatException();
            var loopsCount = ms.ReadB();
            _scaleFlags = ms.ReadB();
            _unknown1 = ms.ReadB();

            _scaleRes = ms.ReadB();
            var totalNumberOfCells = ms.ReadUShortBE();
            var palOffset = ms.ReadUIntBE();
            var loopHeaderSize = ms.ReadB();
            if (loopHeaderSize < 16) throw new FormatException();

            var cellHeaderSize = ms.ReadB();
            if (cellHeaderSize != 36 && cellHeaderSize != 52) throw new FormatException();

            _nativeResolution = ms.ReadUShortBE();

            if (palOffset > 0)
            {
                ms.Position = palOffset;
                Palette = Palette.Read(ms);
            }

            Loops = new List<Loop>(loopsCount);
            for (int l = 0; l < loopsCount; l++)
            {
                var loop = new Loop();
                Loops.Add(loop);

                ms.Position = headerSize + 2 + loopHeaderSize * l;

                loop.LoopMirror = ms.ReadB();
                loop.IsMirror = ms.ReadB() > 0;
                var cellCount = ms.ReadB();
                ms.Position += 9;
                var cellOffset = ms.ReadUIntBE();

                if (!loop.IsMirror)
                {
                    for (int c = 0; c < cellCount; c++)
                    {
                        var cell = new Cell(Package, Palette);
                        cell.ReadVGA11(data, (int)(cellOffset + cellHeaderSize * c));
                        loop.Cells.Add(cell);
                    }
                }
                else
                {
                    foreach (var c in Loops[loop.LoopMirror].Cells)
                    {
                        var cell = new Cell(Package, Palette);
                        loop.Cells.Add(cell);
                        cell.Width = c.Width;
                        cell.Height = c.Height;
                        cell.X = (short)-c.X;
                        cell.Y = c.Y;
                        cell.Pixels = new byte[cell.Width * cell.Height];

                        for (int y = 0; y < cell.Height; y++)
                            for (int x = 0; x < cell.Width; x++)
                                cell.Pixels[y * cell.Width + x] = c.Pixels[y * cell.Width + (cell.Width - 1 - x)];
                    }
                }

            }
        }
        public byte[] GetBytesVGA11()
        {
            ByteBuilder bb = new();
            bb.AddShortBE(0x10); // header size

            if (Loops.Count > 255) throw new Exception("Too many loops");
            bb.AddByte((byte)Loops.Count);
            bb.AddByte(_scaleFlags);
            bb.AddByte(_unknown1);
            bb.AddByte(_scaleRes);
            bb.AddUShortBE((ushort)Loops.Sum(l => l.Cells.Count));
            var palRef = bb.Position;
            bb.AddIntBE(0); // Palette offset
            bb.AddByte(0x10); // Loop header size
            bb.AddByte(0x24); // Cell header size
            bb.AddUShortBE(_nativeResolution);
            bb.AddShortBE(0); // Unknown

            // Write loop headers
            int[] cellsRef = new int[Loops.Count];
            for (int i = 0; i < Loops.Count; i++)
            {
                var loop = Loops[i];
                if (loop.IsMirror)
                {
                    bb.AddByte(loop.LoopMirror);
                    bb.AddByte(1);
                }
                else
                {
                    bb.AddByte(0xff);
                    bb.AddByte(0);
                }
                bb.AddByte((byte)loop.Cells.Count);

                // Constants
                bb.AddByte(0xff);
                bb.AddIntBE(0x3ffffff);
                bb.AddIntBE(0);

                cellsRef[i] = bb.Position;
                bb.AddIntBE(0); // Cell offset
            }

            // Write cells
            var rle = new List<byte>();
            var literals = new List<byte>();
            var cellsOffsets = new List<int>(); // Cell offsets
            var rleOffsets = new List<int>(); // RLE offsets in list
            var literalsOffsets = new List<int>(); // Literals offsets in list
            for (int i = 0; i < Loops.Count; i++)
            {
                var loop = Loops[i];
                for (int j = 0; j < loop.Cells.Count; j++)
                {
                    var cell = loop.Cells[j];

                    // Setup first cell offset
                    if (j == 0) bb.SetIntBE(cellsRef[i], bb.Position);

                    bb.AddUShortBE(cell.Width);
                    bb.AddUShortBE(cell.Height);
                    bb.AddShortBE(cell.X);
                    bb.AddShortBE(cell.Y);
                    bb.AddByte(cell.TransparentColor);
                    bb.AddByte((byte)(cell.NoRLE ? 0 : 0x0a));
                    bb.AddShortBE(0);

                    if (cell.NoRLE)
                    {
                        bb.AddIntBE(cell.Pixels.Length); // Total size
                        bb.AddIntBE(0); // RLE size
                        bb.AddIntBE(cell.PaletteOffset);

                        rleOffsets.Add(rle.Count);
                        rle.AddRange(cell.Pixels);
                        literalsOffsets.Add(-1); // For skip literals set
                        literals.AddRange(Array.Empty<byte>());

                        cellsOffsets.Add(bb.Position);
                        bb.AddIntBE(0); // RLE offset
                        bb.AddIntBE(0); // Literals offset
                        bb.AddIntBE(0); // Per row offset
                    }
                    else
                    {
                        var bbRLE = new ByteBuilder();
                        var bbLit = new ByteBuilder();

                        cell.Write(bbRLE, bbLit);
                        var rleData = bbRLE.GetArray();
                        var litData = bbLit.GetArray();

                        rleOffsets.Add(rle.Count);
                        rle.AddRange(rleData);
                        literalsOffsets.Add(literals.Count);
                        literals.AddRange(litData);

                        bb.AddIntBE(rleData.Length + litData.Length); // Total size
                        bb.AddIntBE(rleData.Length); // RLE size
                        bb.AddIntBE(cell.PaletteOffset);

                        cellsOffsets.Add(bb.Position);
                        bb.AddIntBE(0); // RLE offset
                        bb.AddIntBE(0); // Literals offset
                        bb.AddIntBE(0); // Per row offset
                    }
                }
            }

            // Write palette
            if (Palette != null)
            {
                bb.SetIntBE(palRef, bb.Position);
                Palette.Write(bb);
            }

            // Setup RLE offsets
            for (int i = 0; i < cellsOffsets.Count; i++)
            {
                int offset = cellsOffsets[i];
                bb.SetIntBE(offset, bb.Position + rleOffsets[i]); // RLE offset
            }
            bb.AddBytes(rle);

            for (int i = 0; i < literalsOffsets.Count; i++)
            {
                var lo = literalsOffsets[i];
                if (lo == -1) continue;
                int offset = cellsOffsets[i] + 4;
                bb.SetIntBE(offset, bb.Position + lo); // Literal offset
            }
            bb.AddBytes(literals);

            return bb.GetArray();
        }

        public void ReadEGA(byte[] data)
        {
            MemoryStream ms = new(data);
            var loopsCount = ms.ReadByte();
            if (loopsCount == 0) throw new FormatException();

            var flags = ms.ReadByte();
            var hasPalette = (flags & 0x80) != 0;
            var compressed = (flags & 0x40) != 0;
            var hasBones = (flags & 0x20) != 0;

            var mirrorBits = ms.ReadUShortBE();
            ms.Position += 2;
            var palOffset = ms.ReadUShortBE();

            ushort[] loopOffsets = new ushort[loopsCount];
            for (int i = 0; i < loopsCount; i++)
                loopOffsets[i] = ms.ReadUShortBE();

            if (hasPalette && palOffset > 0)
            {
                ms.Position = palOffset;
                Palette = Palette.Read(ms);
            }
            else
                Palette = Palette.EGA;

            Loops = new List<Loop>(loopsCount);
            for (int i = 0; i < loopsCount; i++)
            {
                var loop = new Loop();
                Loops.Add(loop);

                ms.Position = loopOffsets[i];
                var cellsCount = ms.ReadUShortBE();
                ms.Position += 2; // Skip unknown
                ushort[] cellOffsets = new ushort[cellsCount];
                for (int j = 0; j < cellsCount; j++)
                    cellOffsets[j] = ms.ReadUShortBE();

                for (int j = 0; j < cellsCount; j++)
                {
                    ms.Position = cellOffsets[j];
                    var cell = new Cell(Package, Palette);
                    cell.ReadEVGA(ms, false, 0);
                    loop.Cells.Add(cell);
                }
            }
        }
    }
}
