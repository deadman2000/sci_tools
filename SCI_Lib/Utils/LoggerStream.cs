using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCI_Lib.Utils
{
    class LoggerStream : MemoryStream
    {
        public LoggerStream(byte[] buffer)
            : base(buffer)
        {
        }

        public LoggerStream(byte[] buffer, bool writable)
            : base(buffer, writable)
        {
        }

        private bool _logEnabled = true;

        private string _descr;
        public void Description(string desc) => _descr = desc;
        private static Dictionary<long, string> _regions = new Dictionary<long, string>();

        public static void WriteLog(string path)
        {
            if (_regions.Count == 0) return;

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            using var sw = new StreamWriter(fs);

            foreach (var key in _regions.Keys.OrderBy(k => k))
            {
                sw.WriteLine($"[{key:X4}]: {_regions[key]}");
            }
            _regions.Clear();
        }


        public override void WriteByte(byte value)
        {
            if (_logEnabled) _regions[Position] = $"{value:X2}\t{_descr}";
            base.WriteByte(value);
        }

        public void WriteUShortBE(ushort val)
        {
            _regions[Position] = $"{val:X4}\t{_descr}";
            _logEnabled = false;
            StreamExtensions.WriteUShortBE(this, val);
            _logEnabled = true;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _regions[Position] = $"{BitConverter.ToString(buffer, offset, count).Replace("-", "")}\t{_descr}";
            base.Write(buffer, offset, count);
        }



        public override int ReadByte()
        {
            var pos = Position;
            var value = base.ReadByte();
            if (_logEnabled) _regions[pos] = $"{value:X2}\t{_descr}";
            return value;
        }

        public ushort ReadUShortBE()
        {
            var pos = Position;
            _logEnabled = false;
            var value = StreamExtensions.ReadUShortBE(this);
            _logEnabled = true;
            _regions[pos] = $"{value:X4}\t{_descr}";
            return value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Position;
            var res = base.Read(buffer, offset, count);
            _regions[pos] = $"{BitConverter.ToString(buffer, offset, count).Replace("-", "")}\t{_descr}";
            return res;
        }
    }
}
