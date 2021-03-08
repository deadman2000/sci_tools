using NUnit.Framework;
using SCI_Lib.Utils;
using System.IO;

namespace Tests
{
    public class BitTest
    {
        [Test]
        public void ReadOneMSB()
        {
            var buff = new byte[1] { 0x80 };
            var mem = new MemoryStream(buff);
            BitReaderMSB reader = new BitReaderMSB(mem);
            var val = reader.GetBits(1);
            Assert.AreEqual(1, val);
        }

        [Test]
        public void WriteOneMSB()
        {
            var buff = new byte[1];
            var mem = new MemoryStream(buff);
            BitWriterMSB writer = new BitWriterMSB(mem);
            writer.WriteBits(1, 1);
            writer.Flush();
            Assert.AreEqual(0x80, buff[0]);
        }



        [Test]
        public void ReadTwoMSB()
        {
            var buff = new byte[1] { 0x40 };
            var mem = new MemoryStream(buff);
            BitReaderMSB reader = new BitReaderMSB(mem);
            reader.GetBits(1);
            var val = reader.GetBits(1);
            Assert.AreEqual(1, val);
        }

        [Test]
        public void WriteTwoMSB()
        {
            var buff = new byte[1];
            var mem = new MemoryStream(buff);
            BitWriterMSB writer = new BitWriterMSB(mem);
            writer.WriteBits(0, 1);
            writer.WriteBits(1, 1);
            writer.Flush();
            Assert.AreEqual(0x40, buff[0]);
        }


        [Test]
        public void ReadNineMSB()
        {
            var buff = new byte[] { 0, 0x80 };
            var mem = new MemoryStream(buff);
            BitReaderMSB reader = new BitReaderMSB(mem);
            var val = reader.GetBits(9);
            Assert.AreEqual(1, val);
        }

        [Test]
        public void ReadOneLSB()
        {
            var buff = new byte[1] { 0x01 };
            var mem = new MemoryStream(buff);
            BitReaderLSB reader = new BitReaderLSB(mem);
            var val = reader.GetBits(1);
            Assert.AreEqual(1, val);
        }

        [Test]
        public void WriteOneLSB()
        {
            var buff = new byte[1];
            var mem = new MemoryStream(buff);
            BitWriterLSB writer = new BitWriterLSB(mem);
            writer.WriteBits(1, 1);
            writer.Flush();
            Assert.AreEqual(1, buff[0]);
        }

        [Test]
        public void ReadWriteLSB()
        {
            byte[] buff = new byte[0x1ff * 9 / 8 + 2];
            var mem = new MemoryStream(buff);

            BitWriterLSB writer = new BitWriterLSB(mem);
            for (int i = 0; i <= 0x1ff; i++)
            {
                writer.WriteBits(i, 9);
            }
            writer.Flush();

            mem.Seek(0, SeekOrigin.Begin);
            BitReaderLSB reader = new BitReaderLSB(mem);
            for (int i = 0; i <= 0x1ff; i++)
            {
                var val = reader.GetBits(9);
                Assert.AreEqual(i, val);
            }
        }

        [Test]
        public void ReadWriteMSB()
        {
            for (int n = 4; n <= 10; n++)
            {
                var max = (1 << n) - 1;

                byte[] buff = new byte[max * n / 8 + 2];
                var mem = new MemoryStream(buff);

                BitWriterMSB writer = new BitWriterMSB(mem);
                for (int i = 0; i <= max; i++)
                {
                    writer.WriteBits(i, n);
                }
                writer.Flush();

                mem.Seek(0, SeekOrigin.Begin);
                BitReaderMSB reader = new BitReaderMSB(mem);
                for (int i = 0; i <= max; i++)
                {
                    var val = reader.GetBits((ushort)n);
                    Assert.AreEqual(i, val);
                }
            }
        }
    }
}
