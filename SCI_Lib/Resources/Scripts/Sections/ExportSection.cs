﻿using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Utils;
using System;

namespace SCI_Lib.Resources.Scripts.Sections
{
    class ExportSection : Section
    {
        private bool _isExportWide;

        public override void Read(byte[] data, ushort offset, int length)
        {
            int cnt = ReadShortBE(data, ref offset);
            if (length - 2 == cnt * 4)
                _isExportWide = true;
            else if (length - 2 == cnt * 2)
                _isExportWide = false;
            else
                throw new FormatException();

            Exports = new ExportRef[cnt];
            for (int i = 0; i < cnt; i++)
            {
                var addr = offset;
                var val = ReadShortBE(data, ref offset);

                if (_isExportWide)
                {
                    var zero = ReadShortBE(data, ref offset);
                    if (zero != 0) throw new FormatException();
                }

                if (val != 0)
                    Exports[i] = new ExportRef(_script, addr, val) { Source = this };
            }
        }

        public ExportRef[] Exports { get; private set; }

        public override void Write(ByteBuilder bb)
        {
            bb.AddUShortBE((ushort)Exports.Length);
            foreach (ExportRef exp in Exports)
            {
                if (exp != null)
                    exp.Write(bb);
                else
                    bb.AddShortBE(0);

                if (_isExportWide)
                    bb.AddShortBE(0);
            }
        }
    }
}
