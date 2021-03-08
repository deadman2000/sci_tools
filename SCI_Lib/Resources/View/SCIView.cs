using SCI_Lib.Utils;
using System.Collections.Generic;
using System.IO;

namespace SCI_Lib.Resources.View
{
    public class SCIView
    {
        public SCIView(byte[] data)
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
                    var cell = new Cell();
                    loop.Cells.Add(cell);

                    ms.Position = cellOffsets[j];

                    cell.Width = ms.ReadUShortBE();
                    cell.Height = ms.ReadUShortBE();
                    cell.PlacementX = ms.ReadB();
                    cell.PlacementY = ms.ReadB();
                    cell.TransparentColor = ms.ReadB();
                    ms.Position++; // Skip unknown

                    // TODO Read RLE
                }
            }

            ms.Position = palOffset + 256 + 4;
            Palette = ms.ReadBytes(256 * 4);
        }

        public byte[] Palette { get; set; }

        public List<Loop> Loops { get; } = new List<Loop>();
    }
}
