using SCI_Lib.Resources.Picture;
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
        private byte _scaleFlags;
        private byte _scaleRes;
        private ushort _nativeResolution;

        public SCIPackage Package { get; }

        public Palette Palette { get; set; }

        public List<Loop> Loops { get; private set; }

        public SCIView(SCIPackage package)
        {
            Package = package;
        }

        public void ReadVGA(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            var loopsCount = ms.ReadB();
            var const80 = ms.ReadB();
            if (const80 != 0x80) throw new FormatException();
            _mask = ms.ReadUShortBE();
            var unknown = ms.ReadUShortBE();
            var palOffset = ms.ReadUShortBE();

            ushort[] offsets = new ushort[loopsCount];
            for (int i = 0; i < loopsCount; i++)
                offsets[i] = ms.ReadUShortBE();

            Loops = new List<Loop>(loopsCount);
            for (int i = 0; i < loopsCount; i++)
            {
                var loop = new Loop();
                Loops.Add(loop);

                ms.Position = offsets[i];
                var cellsCount = ms.ReadUShortBE();
                ms.Position += 2; // Skip unknown
                ushort[] cellOffsets = new ushort[cellsCount];
                for (int j = 0; j < cellsCount; j++)
                    cellOffsets[j] = ms.ReadUShortBE();

                for (int j = 0; j < cellsCount; j++)
                {
                    var cell = new Cell(this);
                    loop.Cells.Add(cell);

                    ms.Position = cellOffsets[j];

                    cell.Width = ms.ReadUShortBE();
                    cell.Height = ms.ReadUShortBE();
                    cell.X = ms.ReadB();
                    cell.Y = ms.ReadB();
                    cell.TransparentColor = ms.ReadB();
                    ms.Position++; // Skip unknown

                    cell.Pixels = new byte[cell.Width * cell.Height];
                    ImageEncoder.ReadImage(ms, ms, cell.Pixels, cell.TransparentColor);
                }
            }

            ms.Position = palOffset;
            Palette = Palette.Read(ms);
        }

        public byte[] GetBytesVGA()
        {
            ByteBuilder bb = new ByteBuilder();

            if (Loops.Count > 255) throw new Exception("Too many loops");
            bb.AddByte((byte)Loops.Count);
            bb.AddByte(80);
            bb.AddShortBE(_mask);
            bb.AddShortBE(0);

            var palRefOffest = bb.Position;

            throw new NotImplementedException();
            //return bb.GetArray();
        }

        public void ReadVGA11(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            var headerSize = ms.ReadUShortBE();
            if (headerSize < 14) throw new FormatException();
            var loopsCount = ms.ReadB();
            _scaleFlags = ms.ReadB();
            var unknown = ms.ReadB();

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
                        var cell = new Cell(this);
                        loop.Cells.Add(cell);

                        ms.Position = cellOffset + cellHeaderSize * c;

                        cell.Width = ms.ReadUShortBE();
                        cell.Height = ms.ReadUShortBE();
                        cell.X = ms.ReadShortBE();
                        cell.Y = ms.ReadShortBE();
                        cell.TransparentColor = ms.ReadB();

                        var always_0xa = ms.ReadB();
                        var temp2 = ms.ReadB();
                        var temp3 = ms.ReadB();
                        var totalCellDataSize = ms.ReadUIntBE();
                        var rleCellDataSize = ms.ReadUIntBE();
                        cell.PaletteOffset = ms.ReadUIntBE();
                        var offsetRLE = ms.ReadUIntBE();
                        if (offsetRLE == 0) throw new FormatException();
                        var offsetLiteral = ms.ReadUIntBE();
                        var perRowOffsets = ms.ReadUIntBE();
                        if (always_0xa == 0xa || always_0xa == 0x8a)
                        {
                            if (offsetLiteral == 0) throw new FormatException();
                        }
                        else
                        {
                            if (offsetLiteral != 0) throw new FormatException();
                        }

                        ms.Position = offsetRLE;
                        if (offsetLiteral == 0)
                        {
                            cell.Pixels = ms.ReadBytes(cell.Width * cell.Height);
                        }
                        else
                        {
                            ms.Position = offsetRLE;
                            var msLiteral = new MemoryStream(data);
                            msLiteral.Position = offsetLiteral;
                            cell.Pixels = new byte[cell.Width * cell.Height];
                            ImageEncoder.ReadImage(ms, msLiteral, cell.Pixels, cell.TransparentColor);
                        }
                    }
                }
                else
                {
                    foreach (var c in Loops[loop.LoopMirror].Cells)
                    {
                        var cell = new Cell(this);
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
            ByteBuilder bb = new ByteBuilder();
            bb.AddShortBE(0x10); // header size

            if (Loops.Count > 255) throw new Exception("Too many loops");
            bb.AddByte((byte)Loops.Count);
            bb.AddByte(_scaleFlags);
            bb.AddByte(1);
            bb.AddByte(_scaleRes);
            bb.AddShortBE((ushort)Loops.Sum(l => l.Cells.Count));
            var palRef = bb.Position;
            bb.AddIntBE(0); // Palette offset
            bb.AddByte(0x10); // Loop header size
            bb.AddByte(0x24); // Cell header size
            bb.AddShortBE(_nativeResolution);
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

                    bb.AddShortBE(cell.Width);
                    bb.AddShortBE(cell.Height);
                    bb.AddShortBE(cell.X);
                    bb.AddShortBE(cell.Y);
                    bb.AddByte(cell.TransparentColor);
                    bb.AddByte(0x0a);
                    bb.AddShortBE(0);

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
                int offset = cellsOffsets[i] + 4;
                bb.SetIntBE(offset, bb.Position + literalsOffsets[i]); // Literal offset
            }
            bb.AddBytes(literals);

            return bb.GetArray();
        }

    }
}
