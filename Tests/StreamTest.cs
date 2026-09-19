using NUnit.Framework;
using NUnit.Framework.Legacy;
using SCI_Lib.Utils;
using System.IO;

namespace Tests
{
    class StreamTest
    {
        [Test]
        public void IntTest()
        {
            const int V = 0x01020304;

            MemoryStream mem = new MemoryStream();
            mem.WriteIntBE(V);

            mem.Position = 0;
            var val = mem.ReadIntBE();

            ClassicAssert.AreEqual(V, val);
        }

        [Test]
        public void UShortBETest()
        {
            MemoryStream mem = new MemoryStream();
            mem.WriteUShortBE(1);

            mem.Position = 0;
            var val = mem.ReadUShortBE();

            ClassicAssert.AreEqual(1, val);
        }

        [Test]
        public void UShortLETest()
        {
            MemoryStream mem = new MemoryStream();
            mem.WriteUShortLE(1);

            mem.Position = 0;
            var val = mem.ReadUShortLE();

            ClassicAssert.AreEqual(1, val);
        }
    }
}
