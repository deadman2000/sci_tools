using SCI_Lib.Utils;
using System;
using System.IO;

namespace SCI_Lib.Resources.Picture
{
    class PicImage : PicExtCommand
    {
        private PointShort _coord;
        private byte _transpCol;

        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public byte[] Image { get; set; }

        private const bool LOG = false;
        private static bool WRITE_BY_ROW = true; // Построчная запись

        public PicImage(Stream stream) : base(0x01)
        {
            _coord = stream.ReadPicAbsCoord();
            ushort size = stream.ReadUShortBE();
            var startPos = stream.Position;

            Width = stream.ReadUShortBE();
            Height = stream.ReadUShortBE();
            stream.Seek(2, SeekOrigin.Current);
            _transpCol = stream.ReadB();
            stream.Seek(1, SeekOrigin.Current);

            Image = new byte[Width * Height];

            ReadImageData(stream);
            var readSize = stream.Position - startPos;

            if (size != readSize)
                throw new FormatException();
        }

        private void ReadImageData(Stream stream)
        {
            ReadImageData(stream, stream, Image, _transpCol);
        }


#pragma warning disable CS0162 // Unreachable code detected
        public static void ReadImageData(Stream rle, Stream literal, byte[] img, byte transpColor)
        {
            int ind = 0;
            int addCount = 0;
            while (ind < img.Length)
            {
                var d = rle.ReadB();
                var cnt = (d & 0x3f) + addCount; // 2 бита - код, 6 - количество
                var code = d >> 6;
                if (LOG) Console.Write($"{code} x{cnt} ");

                switch (code)
                {
                    case 0: // Разные пиксели
                        if (LOG) Console.Write("[");
                        for (var i = 0; i < cnt; i++)
                        {
                            img[ind + i] = literal.ReadB();
                            if (LOG) Console.Write($"{img[ind + i]:X2} ");
                        }
                        ind += cnt;
                        addCount = 0;
                        if (LOG) Console.WriteLine("]");
                        break;

                    case 1: // Увеличиваем счетчик
                        addCount += 64;
                        if (LOG) Console.WriteLine();
                        break;

                    case 2: // Одинаковые пиксели подряд
                        var c = literal.ReadB();
                        if (LOG) Console.WriteLine($"{c:X2}");
                        for (var i = 0; i < cnt; i++)
                            img[ind + i] = c;
                        ind += cnt;
                        addCount = 0;
                        break;

                    case 3: // Прозрачные пиксели подряд
                        if (LOG) Console.WriteLine("T");
                        for (var i = 0; i < cnt; i++)
                            img[ind + i] = transpColor;
                        ind += cnt;
                        addCount = 0;
                        break;
                }
            }
        }
#pragma warning restore CS0162 // Unreachable code detected

        protected override void WriteExt(ByteBuilder bb)
        {
            bb.WritePicAbsCoord(ref _coord);
            var sizePos = bb.Position;
            bb.AddShortBE(0); // Потом вернемся, чтобы записать размер данных

            bb.AddShortBE(Width);
            bb.AddShortBE(Height);
            bb.AddShortBE(0);
            bb.AddByte(_transpCol);
            bb.AddByte(0);
            WriteImageData(bb);

            var endPos = bb.Position;
            var size = endPos - sizePos - 2;
            if (size > 0xffff)
                throw new FormatException("Too big image data");

            bb.SetShortBE(sizePos, (ushort)size);
        }

        static bool USE_ADD_COUNT = false;

        private void WriteImageData(ByteBuilder bb)
        {
            if (WRITE_BY_ROW)
            {
                for (int y = 0; y < Height; y++)
                {
                    byte[] row = new byte[Width];
                    Array.Copy(Image, y * Width, row, 0, Width);
                    WriteImageRow(bb, row);
                }
            }
            else
            {
                WriteImageRow(bb, Image);
            }
        }

        private void WriteImageRow(ByteBuilder bb, byte[] row)
        {
            for (int i = 0; i < row.Length; i++) // Current pixel
            {
                var curr = row[i];

                if (i + 1 < row.Length) // Это не последний пиксель 
                {
                    if (row[i + 1] == curr) // ... и есть повторения
                    {
                        var end = i + 1;
                        while (end + 1 < row.Length && row[end + 1] == curr && (USE_ADD_COUNT || end - i + 1 < 0x3f)) end++; // Двигаемся вперед пока пиксели совпадают
                        // На выходе end - указатель на последний повторяющийся пиксель, который мы будем записывать

                        int cnt = end - i + 1;

                        if (end >= row.Length)
                            throw new Exception();

                        if (curr == _transpCol)
                        {
                            WriteCode(bb, 3, cnt);
                            if (LOG) Console.WriteLine("T");
                        }
                        else
                        {
                            WriteCode(bb, 2, cnt);
                            bb.AddByte(curr);
                            if (LOG) Console.WriteLine($"{curr:X}");
                        }

                        i = end;
                    }
                    else // ... пиксели разные
                    {
                        // Ищем количество неповторений
                        // Двигаемся вперед пока пиксели меняются
                        var end = i + 1;
                        int cnt = 2;
                        while (end + 1 < row.Length
                            && (!IsEquals(row, end + 1, cnt < 3 ? 2 : 3))
                            && (USE_ADD_COUNT || end - i + 1 < 0x3f))
                        {
                            end++;
                            cnt++;
                        }

                        WriteCode(bb, 0, cnt);
                        if (LOG) Console.Write("[");
                        for (int n = i; n <= end; n++)
                        {
                            bb.AddByte(row[n]);
                            if (LOG) Console.Write($"{row[n]:X2} ");
                        }
                        if (LOG) Console.WriteLine("]");

                        i = end;
                    }
                }
                else // Последний пиксель
                {
                    WriteCode(bb, 0, 1);
                    bb.AddByte(curr);
                }
            }
        }

        private void WriteCode(ByteBuilder bb, int code, int count)
        {
            if (LOG) Console.Write($"{code} x{count} ");
            while (count > 0x3f)
            {
                bb.AddByte(0x40);
                count -= 64;
            }
            bb.AddByte((byte)((code << 6) | count));
        }

        /// <summary>
        /// Возвращает true, если с указанного индекса идут count одинаковых пикселей
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private bool IsEquals(byte[] row, int index, int count)
        {
            if (index + count >= row.Length) return false;
            var c = row[index];
            for (int i = index + 1; i < index + count; i++)
                if (row[i] != c) return false;

            return true;
        }

        public byte[] GetBytes()
        {
            ByteBuilder bb = new ByteBuilder();
            WriteExt(bb);
            return bb.GetArray();
        }
    }
}
