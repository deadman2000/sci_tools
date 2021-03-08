using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCI_Lib.Compression
{
    public static class ViewEncoder
    {
        public static byte[] EncodeView(byte[] data)
        {
            byte[] result = new byte[data.Length];
            var pixData = new byte[0xFFFF];
            var pixWriter = new MemoryStream(pixData);

            var writer = new MemoryStream(result);
            var rleWriter = new MemoryStream(result);

            var seeker = new MemoryStream(data, false);
            var celHeader = new MemoryStream(data, false);

            var looperheads = seeker.ReadB();

            seeker.Position++;

            var lhMask = seeker.ReadUShortBE();
            var unknown = seeker.ReadUShortBE();
            var palOffset = seeker.ReadUShortBE();

            ushort[] loops = new ushort[looperheads];
            for (int i = 0; i < looperheads; i++)
                loops[i] = seeker.ReadUShortBE();

            var cellcounts = new List<byte>();
            var cc_pos = new List<ushort>();
            var cc_size = new List<ushort>();
            ushort w, h;

            var lhPresent = BitCount(lhMask, looperheads);

            int lb = 1;

            writer.Position = 0xC + lhPresent;
            for (int l = 0; l < looperheads; l++)
            {
                if ((lhMask & lb) != 0)
                {
                }
                else
                {
                    seeker.Position = loops[l];
                    var cnt = (byte)seeker.ReadUShortBE();
                    cellcounts.Add(cnt);
                    seeker.Position += 2; // const 0000

                    for (int j = 0; j < cnt; j++)
                    {
                        var adr = seeker.ReadUShortBE();
                        cc_pos.Add(adr);

                        celHeader.Position = adr;
                        writer.WriteUShortBE(w = celHeader.ReadUShortBE());
                        writer.WriteUShortBE(h = celHeader.ReadUShortBE());
                        writer.WriteUShortBE(celHeader.ReadUShortBE());
                        writer.WriteByte((byte)celHeader.ReadUShortBE());
                        cc_size.Add((ushort)(w * h));
                    }
                }

                lb <<= 1;
            }

            bool exchange = false;
            // Palette
            if (palOffset > 0)
            {
                seeker.Position = palOffset - 3;
                var header = seeker.ReadBytes(3);

                seeker.Position = palOffset + 256 + 4;
                writer.Write(seeker.ReadBytes(4 * 256));

                if (header[0] != 'P' && header[0] != 'A' && header[0] != 'L')
                {
                    palOffset += 3;
                    exchange = true;
                }
            }
            var lenOffset = writer.Position;

            // RLE
            rleWriter.Position = writer.Position + cc_pos.Count * 2;
            List<byte[]> pixDataAll = new List<byte[]>();
            ushort[] cc_length = new ushort[cc_pos.Count];
            for (int i = 0; i < cc_pos.Count; i++)
            {
                seeker.Position = cc_pos[i] + 8;

                pixWriter.Position = 0;
                cc_length[i] = EncodeRLE(seeker, rleWriter, pixWriter, cc_size[i]);
                var arr = new byte[pixWriter.Position];
                Array.Copy(pixData, arr, arr.Length);
                pixDataAll.Add(arr);
            }

            // PIX
            for (int i = 0; i < pixDataAll.Count; i++)
            {
                rleWriter.Write(pixDataAll[i]);
            }

            // cc_length
            writer.Position = lenOffset;
            for (int i = 0; i < cc_length.Length; i++)
            {
                writer.WriteUShortBE(cc_length[i]);
            }
            
            writer.Position = 0;
            writer.WriteUShortBE((ushort)(lenOffset - 2));
            writer.WriteByte(looperheads);
            writer.WriteByte((byte)cellcounts.Count); // lhPresent
            writer.WriteUShortBE(lhMask);
            writer.WriteUShortBE(unknown);
            writer.WriteUShortBE(palOffset);
            writer.WriteUShortBE((ushort)cellcounts.Sum(c => c)); // celTotal
            writer.Write(cellcounts.ToArray());

            if (exchange)
            {
                byte[] exch = new byte[result.Length + 3];
                Array.Copy(result, exch, result.Length);
                return exch;
            }

            return result;
        }

        private static ushort BitCount(ushort lhMask, ushort size)
        {
            int cnt = 0;
            while (size > 0)
            {
                if ((lhMask & 1) == 0)
                    cnt++;
                lhMask >>= 1;
                size--;
            }
            return (ushort)cnt;
        }

        private static ushort EncodeRLE(MemoryStream reader, MemoryStream rleWriter, MemoryStream pixWriter, ushort pixSize)
        {
            ushort pos = 0;
            int pixels = 0;
            while (pixels < pixSize)
            {
                var nextByte = reader.ReadB();
                rleWriter.WriteByte(nextByte);
                pixels += nextByte & 0x3f;
                pos++;

                switch (nextByte & 0xC0)
                {
                    case 0x00:
                        pixWriter.Write(reader.ReadBytes(nextByte));
                        pos += nextByte;
                        break;

                    case 0x40:
                        pixWriter.Write(reader.ReadBytes(nextByte));
                        pixels += 64;
                        pos += nextByte;
                        break;

                    case 0x80:
                        pixWriter.WriteByte(reader.ReadB());
                        pos++;
                        break;

                    default:
                        break;
                }
            }
            return pos;
        }
    }
}
