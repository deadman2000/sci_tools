using NUnit.Framework;
using NUnit.Framework.Legacy;
using SCI_Lib;
using SCI_Lib.Resources;
using System;
using System.IO;
using System.Threading.Tasks;

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

            Parallel.ForEach(package.Resources, r =>
            {
                var r2 = packed.GetResource(r.FileName);
                ClassicAssert.IsNotNull(r2);

                ClassicAssert.AreEqual(r.Number, r2.Number, $"{r.FileName}");

                var i = r.GetInfo();
                var i2 = r2.GetInfo();

                ClassicAssert.AreEqual(i.ResT, i2.ResT, $"{r.FileName}");
                ClassicAssert.AreEqual(i.ResNr, i2.ResNr, $"{r.FileName}");
                ClassicAssert.AreEqual(i.Method, i2.Method, $"{r.FileName}");

                if (i.Method != 0)
                    ClassicAssert.Less(i2.CompSize - 4, i2.DecompSize, $"Compressed size error {r.FileName}");

                if (r.Type != ResType.View)
                {
                    var data = r.GetContent();
                    var data2 = r2.GetContent();
                    ClassicAssert.AreEqual(data, data2, $"{r.FileName}");

                    if (r2.Volumes.Count > 1)
                    {
                        for (int n = 1; n < r2.Volumes.Count; n++)
                        {
                            var dataExt = r2.GetContent(n);
                            ClassicAssert.AreEqual(data, dataExt, $"{r.FileName}");
                        }
                    }
                }
            });
        }

        [TearDown]
        public void CleanUp()
        {
            Directory.Delete(tmp, true);
        }
    }
}
