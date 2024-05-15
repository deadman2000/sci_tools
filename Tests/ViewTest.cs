using NUnit.Framework;
using SCI_Lib.Resources;
using SCI_Lib.Resources.View;
using System.IO;
using System.Linq;

namespace Tests
{
    class ViewTest
    {
        [Test]
        public void SuccessRecompressVGA()
        {
            var package = Utils.LoadConquest();

            //var r = package.GetResource("95.p56");
            foreach (var r in package.GetResources<ResView>().Take(10))
            {
                if (r.Number == 460) continue; // Skip broken view in Conquest
                var info = r.GetInfo();

                if (info.Method != 3) continue;

                var comp = info.GetCompressor();
                var decomp = info.GetDecompressor();

                var unpack = r.GetContent();
                var compressed = comp.Pack(unpack);

                var ms = new MemoryStream(compressed);
                var uncompressed = decomp.Unpack(ms, compressed.Length, unpack.Length);

                //Assert.AreEqual(unpack, uncompressed, $"Decompress error in {r.FileName}");

                var originalView = new SCIView(package);
                originalView.ReadVGA(unpack);
                var reorderView = new SCIView(package);
                reorderView.ReadVGA(uncompressed);

                Assert.AreEqual(originalView.Palette.Colors[3..], reorderView.Palette.Colors[3..]);

                Assert.AreEqual(originalView.Loops.Count, reorderView.Loops.Count);

                for (int i = 0; i < originalView.Loops.Count; i++)
                {
                    var ol = originalView.Loops[i];
                    var rl = reorderView.Loops[i];
                    Assert.AreEqual(ol.Cells.Count, rl.Cells.Count);

                    for (int j = 0; j < ol.Cells.Count; j++)
                    {
                        var oc = ol.Cells[j];
                        var rc = rl.Cells[j];

                        Assert.AreEqual(oc.Width, rc.Width);
                        Assert.AreEqual(oc.Height, rc.Height);
                        Assert.AreEqual(oc.X, rc.X);
                        Assert.AreEqual(oc.Y, rc.Y);
                        Assert.AreEqual(oc.TransparentColor, rc.TransparentColor);
                    }
                }
            }
        }

        [Test]
        public void SaveVGA()
        {
            var package = Utils.LoadEQ();

            foreach (var r in package.GetResources<ResView>())
            {
                var view = r.GetView();

                r.SetView(view);
                var patched = r.GetPatch();
                var view2 = r.GetView(patched);

                CollectionAssert.AreEqual(view.Palette.Colors, view2.Palette.Colors);

                Assert.AreEqual(view.Loops.Count, view2.Loops.Count);
                for (int i = 0; i < view.Loops.Count; i++)
                {
                    var ol = view.Loops[i];
                    var rl = view2.Loops[i];
                    Assert.AreEqual(ol.Cells.Count, rl.Cells.Count);

                    for (int j = 0; j < ol.Cells.Count; j++)
                    {
                        var oc = ol.Cells[j];
                        var rc = rl.Cells[j];

                        Assert.AreEqual(oc.Width, rc.Width);
                        Assert.AreEqual(oc.Height, rc.Height);
                        Assert.AreEqual(oc.X, rc.X);
                        Assert.AreEqual(oc.Y, rc.Y);
                        Assert.AreEqual(oc.TransparentColor, rc.TransparentColor);
                        CollectionAssert.AreEqual(oc.Pixels, rc.Pixels);
                    }
                }
            }
        }
    }
}
