using SCI_Lib.Utils;
using System;
using System.IO;
using System.Text;

namespace SCI_Lib.Resources.Audio
{
    public class AudioSample
    {
        private readonly Stream _stream;
        private long _dataOffset;
        private int _dataSize;

        public AudioSample(Stream stream, int offset, uint number)
        {
            _stream = stream;
            Format = AudioFormat.Unread;
            Offset = offset;
            Number = number;
        }

        public byte Type { get; private set; }
        public AudioFormat Format { get; private set; }
        public long Offset { get; set; }
        public uint Number { get; }
        public int Size { get; private set; }
        public int Rate { get; private set; }
        public SolFlags Flags { get; private set; }

        public override string ToString() => $"{Offset}..{Offset + Size} #{Number:x08}";

        public byte[] GetRaw()
        {
            _stream.Seek(Offset, SeekOrigin.Begin);
            return _stream.ReadBytes(Size);
        }

        public void ReadHeader()
        {
            _stream.Seek(Offset, SeekOrigin.Begin);
            var prefix = Encoding.ASCII.GetString(_stream.ReadBytes(4));
            _stream.Seek(-4, SeekOrigin.Current);

            if (prefix == "RIFF")
            {
                Format = AudioFormat.RIFF;
                ReadRiffHeader();
            }
            else
            {
                Format = AudioFormat.SOL;
                ReadSOLHeader();
            }
        }

        private void ReadSOLHeader()
        {
            Type = _stream.ReadB();
            var headerSize = _stream.ReadB();
            if (headerSize != 12) throw new NotImplementedException();

            var formatTypeBytes = _stream.ReadBytes(4);
            var formatType = Encoding.ASCII.GetString(formatTypeBytes);
            if (formatType != "SOL\0") throw new NotImplementedException();

            Rate = _stream.ReadUShortBE();
            Flags = (SolFlags)_stream.ReadB();
            _dataSize = _stream.ReadIntBE();
            _dataOffset = _stream.Position;
            Size = 2 + headerSize + _dataSize;
        }

        private void ReadRiffHeader()
        {
            _stream.Seek(4, SeekOrigin.Current);
            Size = (int)(_stream.ReadUIntBE() + 8);
        }

        public byte[] ExtractSOL()
        {
            _stream.Seek(_dataOffset, SeekOrigin.Begin);
            return SOLCodec.Decode(_stream, _dataSize);
        }

        public static AudioSample CreateSOL(byte[] data, uint number)
        {
            MemoryStream ms = new();
            ms.WriteByte(0x8d); // type
            ms.WriteByte(12); // header size
            var a = Encoding.ASCII.GetBytes("SOL\0");
            ms.Write(a);
            ms.WriteUShortBE(22050); // Rate
            ms.WriteByte((byte)(SolFlags.Compressed | SolFlags.Is16Bit | SolFlags.IsSigned));
            ms.WriteIntBE(data.Length);
            ms.Write(data);
            var size = ms.Position;
            ms.Seek(0, SeekOrigin.Begin);
            var sample = new AudioSample(ms, 0, number);
            sample.Size = (int)size;
            return sample;
        }
    }
}
