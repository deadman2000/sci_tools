using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Tests
{
    class CompressLZW1Test
    {
        [Test]
        public void SuccessUncompress()
        {
            var package = Utils.LoadPackage();

            //var r = package.GetResouce("95.p56");
            foreach (var r in package.Resources.Take(250))
            {
                var info = r.GetInfo();

                if (info.Method != 2) continue;

                var comp = info.GetCompressor();
                var decomp = info.GetDecompressor();

                var unpack = r.GetContent();

                var compressed = comp.Pack(unpack);

                var ms = new MemoryStream(compressed);
                var uncompressed = decomp.Unpack(ms, compressed.Length, unpack.Length);

                Assert.AreEqual(unpack, uncompressed, $"Decompress error in {r.FileName}");
            }
        }

        //[Test]
        public void CompressedEqual()
        {
            var package = Utils.LoadPackage();

            //var r = package.GetResouce("95.p56");
            foreach (var r in package.Resources)
            {
                var info = r.GetInfo();

                if (info.Method != 2 && info.Method != 3 && info.Method != 4) continue;

                var orig = r.GetCompressed();

                var mem = new MemoryStream();
                info.GetCompressor().Pack(r.GetContent(), mem);
                var compressed = mem.ToArray();

                Assert.IsTrue(compressed.Length <= orig.Length, $"Decompress error in {r.FileName}");
                Array.Resize(ref orig, compressed.Length);

                Assert.AreEqual(orig, compressed, $"Decompress error in {r.FileName}");
            }
        }
    }
}
