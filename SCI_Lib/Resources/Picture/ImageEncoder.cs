using SCI_Lib.Utils;
using System;
using System.IO;

namespace SCI_Lib.Resources.Picture
{
#pragma warning disable CS0162 // Unreachable code detected
    public static class ImageEncoder
    {
        private const bool LOG = false;
        private static StreamWriter logStream;

        private static bool WRITE_BY_ROW = true; // Построчная запись
        private static bool USE_ADD_COUNT = false;

        public static void ReadImageVGA(Stream rle, Stream literal, byte[] img, byte transpColor)
        {
            if (LOG) logStream = new StreamWriter(new FileStream("decoder.log", FileMode.Create));

            int ind = 0;
            int addCount = 0;
            while (ind < img.Length)
            {
                var d = rle.ReadB();

                var cnt = (d & 0x3f) + addCount; // 2 бита - код, 6 - количество
                var code = d >> 6;
                if (LOG) logStream.Write($"[{rle.Position - 1:X}] {code} x{cnt} ");

                switch (code)
                {
                    case 0: // Разные пиксели
                        if (LOG) logStream.Write("[");
                        for (var i = 0; i < cnt; i++)
                        {
                            img[ind + i] = literal.ReadB();
                            if (LOG) logStream.Write($"{img[ind + i]:X2} ");
                        }
                        ind += cnt;
                        addCount = 0;
                        if (LOG) logStream.WriteLine("]");
                        break;

                    case 1: // Увеличиваем счетчик
                        addCount += 64;
                        if (LOG) logStream.WriteLine();
                        break;

                    case 2: // Одинаковые пиксели подряд
                        var c = literal.ReadB();
                        if (LOG) logStream.WriteLine($"{c:X2}");
                        for (var i = 0; i < cnt; i++)
                            img[ind + i] = c;
                        ind += cnt;
                        addCount = 0;
                        break;

                    case 3: // Прозрачные пиксели подряд
                        if (LOG) logStream.WriteLine("T");
                        for (var i = 0; i < cnt; i++)
                            img[ind + i] = transpColor;
                        ind += cnt;
                        addCount = 0;
                        break;
                }
            }

            if (LOG)
            {
                logStream.Flush();
                logStream.Close();
            }
        }

        public static void WriteImageVGA(ByteBuilder bbRLE, ByteBuilder bbLiterals, byte[] image, int width, byte transpCol)
        {
            if (LOG) logStream = new StreamWriter(new FileStream("encoder.log", FileMode.Create));

            if (WRITE_BY_ROW)
            {
                int height = image.Length / width;
                for (int y = 0; y < height; y++)
                {
                    WriteImageRow(bbRLE, bbLiterals, image, y * width, width, transpCol);
                }
            }
            else
            {
                WriteImageRow(bbRLE, bbLiterals, image, 0, image.Length, transpCol);
            }

            if (LOG)
            {
                logStream.Flush();
                logStream.Close();
            }
        }

        public static void ReadImageEGA(Stream rle, byte[] img)
        {
            int ind = 0;
            while (ind < img.Length)
            {
                var d = rle.ReadB();

                byte c = (byte)(d & 0x0f);
                var cnt = (d & 0xf0) >> 4;
                for (var i = 0; i < cnt; i++)
                    img[ind++] = c;
            }
        }

        public static void WriteImageEGA(ByteBuilder rle, byte[] image, int width)
        {
            /*var rows = image.Length / width;
            for (int y = 0; y < rows; y++)
            {
                WriteRowEGA(rle, image, y * width, width);
            }*/

            WriteRowEGA(rle, image, 0, image.Length);
        }


        private static void WriteRowEGA(ByteBuilder rle, byte[] image, int offset, int width)
        {
            int start = 0;
            while (start < width)
            {
                byte color = image[offset + start];
                int cnt = 1;
                while (cnt < 15 && start + cnt + 1 < width && image[offset + start + cnt] == color)
                    cnt++;

                byte b = (byte)((cnt << 4) | color);
                rle.AddByte(b);

                start += cnt;
            }
        }

        private static void WriteImageRow(ByteBuilder bbRLE, ByteBuilder bbLiterals, byte[] data, int offset, int count, byte transpColor)
        {
            for (int i = 0; i < count; i++)
            {
                var curr = data[offset + i];// Current pixel

                if (i + 1 < count) // Это не последний пиксель 
                {
                    if (data[offset + i + 1] == curr) // ... и есть повторения
                    {
                        var end = i + 1;
                        while (end + 1 < count && data[offset + end + 1] == curr && (USE_ADD_COUNT || end - i + 1 < 0x3f)) end++; // Двигаемся вперед пока пиксели совпадают
                        // На выходе end - указатель на последний повторяющийся пиксель, который мы будем записывать

                        int cnt = end - i + 1;

                        if (end >= count)
                            throw new Exception();

                        if (curr == transpColor)
                        {
                            WriteCode(bbRLE, 3, cnt);
                            if (LOG) logStream.WriteLine("T");
                        }
                        else
                        {
                            WriteCode(bbRLE, 2, cnt);
                            bbLiterals.AddByte(curr);
                            if (LOG) logStream.WriteLine($"{curr:X2}");
                        }

                        i = end;
                    }
                    else // ... пиксели разные
                    {
                        // Ищем количество неповторений
                        // Двигаемся вперед пока пиксели меняются
                        var end = i;
                        int cnt = 1;
                        while (end + 1 < count
                            && (!IsEquals(data, offset + end + 1, 3))
                            && (USE_ADD_COUNT || end - i + 1 < 0x3f))
                        {
                            end++;
                            cnt++;
                        }

                        WriteCode(bbRLE, 0, cnt);
                        if (LOG) logStream.Write("[");
                        for (int n = i; n <= end; n++)
                        {
                            bbLiterals.AddByte(data[offset + n]);
                            if (LOG) logStream.Write($"{data[offset + n]:X2} ");
                        }
                        if (LOG) logStream.WriteLine("]");

                        i = end;
                    }
                }
                else // Последний пиксель
                {
                    WriteCode(bbRLE, 0, 1);
                    bbLiterals.AddByte(curr);
                    if (LOG) logStream.WriteLine($"[{curr:X2} ]");
                }
            }
        }

        private static void WriteCode(ByteBuilder bb, int code, int count)
        {
            if (LOG) logStream.Write($"[{bb.Position:X}] {code} x{count} ");
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
        private static bool IsEquals(byte[] row, int index, int count)
        {
            if (index + count >= row.Length) return false;
            var c = row[index];
            for (int i = index + 1; i < index + count; i++)
                if (row[i] != c) return false;

            return true;
        }
    }
#pragma warning restore CS0162 // Unreachable code detected
}
