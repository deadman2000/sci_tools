using SCI_Lib.Utils;
using System;
using System.IO;

namespace SCI_Lib.Decompression
{
    public static class ViewDecoder
    {
        public static byte[] DecodeView(byte[] data)
        {
            byte[] result = new byte[data.Length];

            var writer = new MemoryStream(result);
            var seeker = new MemoryStream(data, false);
      
            var lenOffset = seeker.ReadUShortBE();
            var looperheads = seeker.ReadB();
            var lhPresent = seeker.ReadB();
            var lhMask = seeker.ReadUShortBE();
            var unknown = seeker.ReadUShortBE();
            var palOffset = seeker.ReadUShortBE();
            var celTotal = seeker.ReadUShortBE();
            var celCounts = seeker.ReadBytes(lhPresent);

            var cc_pos = new int[celTotal];

            var cellengths = new MemoryStream(data, false);

            cellengths.Position = lenOffset + 2;
            var cc_lengths = new ushort[celTotal];
            for (int i = 0; i < celTotal; i++)
            {
                cc_lengths[i] = cellengths.ReadUShortBE();
            }

            writer.WriteByte(looperheads);
            writer.WriteByte(0x80);
            writer.WriteUShortBE(lhMask);
            writer.WriteUShortBE(unknown);
            writer.WriteUShortBE(palOffset);

            var lh_ptr = new MemoryStream(result);
            lh_ptr.Position = writer.Position;
            writer.Seek(2 * looperheads, SeekOrigin.Current);

            int lb = 1;
            int celindex = 0;
            int lh_last = -1;

            int w = 0;
            for (int l = 0; l < looperheads; l++)
            {
                if ((lhMask & lb) != 0)
                {
                    if (lh_last == -1) throw new Exception("While reordering view: Loop not present, but can't re-use last loop");

                    lh_ptr.WriteUShortBE((ushort)lh_last);
                }
                else
                {
                    lh_last = (int)writer.Position;

                    lh_ptr.WriteUShortBE((ushort)lh_last);
                    writer.WriteUShortBE(celCounts[w]);
                    writer.WriteUShortBE(0);

                    var chptr = writer.Position + 2 * celCounts[w];

                    for (var c = 0; c < celCounts[w]; c++)
                    {
                        writer.WriteUShortBE((ushort)chptr);
                        cc_pos[celindex + c] = (int)chptr;
                        chptr += 8 + cc_lengths[celindex + c]; // BuildCelHeaders + RLE
                    }

                    for (int c = 0; c < celCounts[w]; c++)
                    {
                        writer.Write(seeker.ReadBytes(6));
                        writer.WriteUShortBE(seeker.ReadB());
                        writer.Seek(cc_lengths[celindex], SeekOrigin.Current);
                        celindex++;
                    }

                    w++;
                }

                lb <<= 1;
            }

            var pix_ptr = new MemoryStream(data, false);
            pix_ptr.Position = lenOffset + 2 + 2 * celTotal;
            for (var c = 0; c < celTotal; c++)
            {
                SkipRLE(pix_ptr, cc_lengths[c]);
            }

            var rle_ptr = new MemoryStream(data, false);
            rle_ptr.Position = lenOffset + 2 + 2 * celTotal;

            for (var c = 0; c < celTotal; c++)
            {
                writer.Position = cc_pos[c] + 8;
                DecodeRLE(rle_ptr, pix_ptr, writer, cc_lengths[c]);
            }

            if (palOffset > 0)
            {
                if (writer.Position != palOffset)
                {
                    writer.WriteByte((byte)'P');
                    writer.WriteByte((byte)'A');
                    writer.WriteByte((byte)'L');
                }

                for (int c = 0; c < 256; c++)
                    writer.WriteByte((byte)c);

                writer.WriteUIntBE(0); // Skip 4 bytes
                writer.Write(seeker.ReadBytes(4 * 256));
            }

            return result;
        }

        private static void SkipRLE(MemoryStream rledata, int dsize)
        {
            int pos = 0;
            while (pos < dsize)
            {
                var nextbyte = rledata.ReadB();
                pos++;

                switch (nextbyte & 0xC0)
                {
                    case 0x40:
                    case 0x00:
                        pos += nextbyte;
                        break;
                    case 0xC0:
                        break;
                    case 0x80:
                        pos++;
                        break;
                }
            }
        }

        private static void DecodeRLE(MemoryStream rle_ptr, MemoryStream pix_ptr, MemoryStream writer, ushort size)
        {
            int pos = 0;
            while (pos < size)
            {
                var nextByte = rle_ptr.ReadB();
                writer.WriteByte(nextByte);
                pos++;
                switch (nextByte & 0xC0)
                {
                    case 0x40:
                    case 0x00:
                        var buff = new byte[nextByte];
                        pix_ptr.Read(buff, 0, nextByte);
                        writer.Write(buff, 0, nextByte);
                        pos += nextByte;
                        break;

                    case 0x80:
                        writer.WriteByte(pix_ptr.ReadB());
                        pos++;
                        break;

                    default:
                        break;
                }
            }
        }

    }
}
