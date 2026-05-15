using NUnit.Framework;
using SCI_Lib.Resources;
using SCI_Lib.Resources.View;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    class ViewTest
    {
        [Test]
        public void SuccessRecompressVGA()
        {
            var package = Utils.LoadConquest();

            var resources = package.GetResources<ResView>().Take(10);
            //var r = package.GetResource("95.p56");
            Parallel.ForEach(resources, r =>
            {
                if (r.Number == 460) return; // Skip broken view in Conquest
                var info = r.GetInfo();

                if (info.Method != 3) return;

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
            });
        }

        [Test]
        public void SaveVGA()
        {
            var package = Utils.LoadEQ();

            var resources = package.GetResources<ResView>().ToList();

            Parallel.ForEach(resources, r =>
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

                        Assert.That(oc.Pixels.AsSpan().SequenceEqual(rc.Pixels.AsSpan()),
                            $"Pixels mismatch in resource {r.Number}, loop {i}, cell {j}");
                    }
                }
            });
        }
    }
}
