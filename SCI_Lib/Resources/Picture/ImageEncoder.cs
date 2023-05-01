using SCI_Lib.Utils;
using System;
using System.IO;

namespace SCI_Lib.Resources.Picture
{
#pragma warning disable CS0162 // Unreachable code detected
    public static class ImageEncoder
    {
        private const bool LOG = false;
        private static bool WRITE_BY_ROW = true; // Построчная запись
        private static bool USE_ADD_COUNT = false;

        public static void ReadImage(Stream rle, Stream literal, byte[] img, byte transpColor, bool isVGA)
        {
            int ind = 0;
            int addCount = 0;
            while (ind < img.Length)
            {
                var d = rle.ReadB();

                if (!isVGA)
                {
                    byte c = (byte)(d & 0x0f);
                    var cnt = (d & 0xf0) >> 4;
                    for (var i = 0; i < cnt; i++)
                        img[ind++] = c;
                }
                else
                {
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
        }

        public static void WriteImage(ByteBuilder bbRLE, ByteBuilder bbLiterals, byte[] image, int width, byte transpCol)
        {
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
                            if (LOG) Console.WriteLine("T");
                        }
                        else
                        {
                            WriteCode(bbRLE, 2, cnt);
                            bbLiterals.AddByte(curr);
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
                        while (end + 1 < count
                            && (!IsEquals(data, offset + end + 1, cnt < 3 ? 2 : 3))
                            && (USE_ADD_COUNT || end - i + 1 < 0x3f))
                        {
                            end++;
                            cnt++;
                        }

                        WriteCode(bbRLE, 0, cnt);
                        if (LOG) Console.Write("[");
                        for (int n = i; n <= end; n++)
                        {
                            bbLiterals.AddByte(data[offset + n]);
                            if (LOG) Console.Write($"{data[offset + n]:X2} ");
                        }
                        if (LOG) Console.WriteLine("]");

                        i = end;
                    }
                }
                else // Последний пиксель
                {
                    WriteCode(bbRLE, 0, 1);
                    bbLiterals.AddByte(curr);
                }
            }
        }

        private static void WriteCode(ByteBuilder bb, int code, int count)
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
