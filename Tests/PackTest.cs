using NUnit.Framework;
using SCI_Lib;
using SCI_Lib.Resources;
using System;
using System.IO;

namespace Tests
{
    class PackTest
    {
        string tmp;

        [SetUp]
        public void Setup()
        {
            tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Console.WriteLine(tmp);
            Directory.CreateDirectory(tmp);
        }

        [Test]
        public void Pack()
        {
            var package = Utils.LoadConquest();
            package.Pack(tmp);

            package = Utils.LoadConquest();
            var packed = SCIPackage.Load(tmp);

            foreach (var r in package.Resources)
            {
                var r2 = packed.GetResource(r.FileName);
                Assert.IsNotNull(r2);

                Assert.AreEqual(r.Number, r2.Number, $"{r.FileName}");

                var i = r.GetInfo();
                var i2 = r2.GetInfo();

                Assert.AreEqual(i.ResT, i2.ResT, $"{r.FileName}");
                Assert.AreEqual(i.ResNr, i2.ResNr, $"{r.FileName}");
                Assert.AreEqual(i.Method, i2.Method, $"{r.FileName}");

                if (i.Method != 0)
                    Assert.Less(i2.CompSize - 4, i2.DecompSize, $"Compressed size error {r.FileName}");

                if (r.Type != ResType.View)
                {
                    var data = r.GetContent();
                    var data2 = r2.GetContent();
                    Assert.AreEqual(data, data2, $"{r.FileName}");

                    if (r2.Volumes.Count > 1)
                    {
                        for (int n = 1; n < r2.Volumes.Count; n++)
                        {
                            var dataExt = r2.GetContent(n);
                            Assert.AreEqual(data, dataExt, $"{r.FileName}");
                        }
                    }
                }
            }
        }

        [TearDown]
        public void CleanUp()
        {
            Directory.Delete(tmp, true);
        }
    }
}
