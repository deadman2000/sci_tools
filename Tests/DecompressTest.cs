using NUnit.Framework;
using SCI_Lib;
using SCI_Lib.Resources;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    public class DecompressTest
    {
        protected Task CheckDecompress(SCIPackage package, string uncompDir, Func<Resource, bool> match = null)
        {
            return Task.WhenAll(package.Resources.Select(async r =>
            {
                if (match != null && !match(r)) return;

                var unpack = r.GetContent();

                var filePath = Path.Combine(uncompDir, r.FileName);

                if (!File.Exists(filePath))
                    filePath = Path.Combine(uncompDir, r.FileName.ToLower());

                if (!File.Exists(filePath))
                    return;

                var target = await File.ReadAllBytesAsync(filePath);

                int offset = Resource.GetResourceOffsetInFile(target[1]);

                // Первые 2 байта - тип ресурса
                var trimmed = new byte[target.Length - 2 - offset];
                Array.Copy(target, 2 + offset, trimmed, 0, trimmed.Length);

                Assert.AreEqual(trimmed, unpack, $"Decompress error in {r.FileName}");
            }));
        }

        [Test]
        public async Task DecompressConquest()
        {
            await CheckDecompress(Utils.LoadConquest(), Path.Combine(Utils.ASSETS, "Conquest_res"), r => r.Type != ResType.View && r.Type != ResType.Picture);
        }

        [Test]
        public async Task DecompressQG_VGA()
        {
            await CheckDecompress(Utils.LoadQG(), Path.Combine(Utils.ASSETS, "QG_VGA_res"));
        }
    }
}
