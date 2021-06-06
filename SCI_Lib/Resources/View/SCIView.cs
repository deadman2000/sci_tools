using SCI_Lib.Resources.Picture;
using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SCI_Lib.Resources.View
{
    public class SCIView
    {
        public SCIPackage Package { get; }

        public Palette Palette { get; set; }

        public List<Loop> Loops { get; } = new List<Loop>();

        public SCIView(SCIPackage package)
        {
            Package = package;
        }

        public void ReadVGA(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            var loopsCount = ms.ReadB();
            ms.Position++; // Skip const 80
            var mask = ms.ReadUShortBE();
            ms.Position += 2; // Skip unkown
            var palOffset = ms.ReadUShortBE();

            ushort[] offsets = new ushort[loopsCount];
            for (int i = 0; i < loopsCount; i++)
                offsets[i] = ms.ReadUShortBE();

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
                    PicImage.ReadImageData(ms, ms, cell.Pixels, cell.TransparentColor);
                }
            }

            ms.Position = palOffset;
            ReadPalette(ms);
        }

        public void ReadVGA11(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            var headerSize = ms.ReadUShortBE();
            if (headerSize < 14) throw new FormatException();
            var loopsCount = ms.ReadB();
            var scaleFlags = ms.ReadB();
            var testb = ms.ReadB();
            //if (testb != 1) throw new FormatException();

            var sci32ScaleRes = ms.ReadB();
            var totalNumberOfCells = ms.ReadUShortBE();
            var palOffset = ms.ReadUIntBE();
            var loopHeaderSize = ms.ReadB();
            if (loopHeaderSize < 16) throw new FormatException();

            var cellHeaderSize = ms.ReadB();
            if (cellHeaderSize != 36 && cellHeaderSize != 52) throw new FormatException();

            var nativeResolution = ms.ReadUShortBE();

            if (palOffset > 0)
            {
                ms.Position = palOffset;
                ReadPalette(ms);
            }

            for (int l = 0; l < loopsCount; l++)
            {
                var loop = new Loop();
                Loops.Add(loop);

                ms.Position = headerSize + 2 + loopHeaderSize * l;

                var mirrorInfo = ms.ReadB();
                var isMirror = ms.ReadB();
                var cellCount = ms.ReadB();
                ms.Position += 9;
                var cellOffset = ms.ReadUIntBE();

                if (mirrorInfo == 255)
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
                        var paletteOffset = ms.ReadUIntBE();
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
                            PicImage.ReadImageData(ms, msLiteral, cell.Pixels, cell.TransparentColor);
                        }
                    }
                }
                else
                {
                    foreach (var c in Loops[mirrorInfo].Cells)
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

        private void ReadPalette(MemoryStream ms)
        {
            var palOffset = ms.Position;

            var marker = ms.ReadB();
            var garbage = ms.ReadBytes(9);
            ms.Position += 2 + 1;
            //if (ms.ReadUShortBE() != 1) throw new FormatException();
            //if (ms.ReadB() != 0) throw new FormatException();

            var offsetEnd = ms.ReadUShortBE();
            var globalMarker = ms.ReadUIntBE();
            ms.Position += 4 + 2;
            //if (ms.ReadUShortBE() != 0) throw new FormatException();
            var colStart = ms.ReadB();
            ms.Position += 3;
            var colCount = ms.ReadUShortBE();

            ms.Position += 1;
            var palFormat = ms.ReadB();

            if (marker == 0 && garbage[0] == 1)
            {
                palFormat = 0;
                colStart = 0;
                colCount = 256;
                palOffset += 260;
            }
            else
            {
                palOffset += 37;
            }

            if (colCount > 0)
            {
                ms.Position = palOffset;

                var end = colStart + colCount;
                if (end > 256) throw new FormatException();

                Palette = new Palette
                {
                    Colors = new Color[256]
                };

                if (palFormat == 0)
                {
                    for (int i = colStart; i < end; i++)
                    {
                        var used = ms.ReadB();
                        var r = ms.ReadB();
                        var g = ms.ReadB();
                        var b = ms.ReadB();
                        if (used > 0)
                            Palette.Colors[i] = Color.FromArgb(r, g, b);
                    }
                }
                else if (palFormat == 1)
                {
                    for (int i = colStart; i < end; i++)
                    {
                        var r = ms.ReadB();
                        var g = ms.ReadB();
                        var b = ms.ReadB();
                        Palette.Colors[i] = Color.FromArgb(r, g, b);
                    }
                }
                else throw new FormatException();
            }
        }
    }
}
