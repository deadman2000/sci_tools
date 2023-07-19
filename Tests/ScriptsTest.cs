using NUnit.Framework;
using SCI_Lib;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System.Linq;

namespace Tests
{
    public class ScriptsTest
    {
        [SetUp]
        public void Setup()
        {
        }
        
        private void CheckPackage(SCIPackage package)
        {
            foreach (var res in package.Scripts)
            {
                var scr = res.GetScript() as Script;
                Assert.IsNotNull(scr);

                CheckScriptValid(scr);
            }
        }

        private void CheckScriptValid(Script scr)
        {
            // Проверка ссылок на ссылки
            foreach (var rel in scr.Sections.OfType<RelocationSection>())
            {
                foreach (var r in rel.Refs)
                {
                    var descr = $"Res: {scr.Resource} Ref at {r.Address:x4} Target: {r.TargetOffset:x4} Source: {r.Source}";
                    Assert.IsNotNull(r.Reference, descr);
                    if (r.Reference is not RefToElement && r.Reference is not PropertyElement)
                        Assert.Fail(descr);
                }
            }
        }

        [Test]
        public void DisassembleAll()
        {
            CheckPackage(Utils.LoadPackage());
        }

        [Test]
        public void ParseAndBack()
        {
            SCIPackage package = Utils.LoadPackage();

            foreach (var res in package.Scripts)
            {
                var bytes = res.GetContent();
                var newbytes = res.GetScript().GetBytes();
                CollectionAssert.AreEqual(bytes, newbytes);
            }
        }

        [Test]
        public void SetAddressOnWrite()
        {
            SCIPackage package = Utils.LoadPackage();
            
            foreach (var r in package.Scripts)
            {
                var scr = r.GetScript() as Script;
                foreach (var e in scr.AllElements)
                    e.IsAddressSet = false;

                scr.GetBytes();

                foreach (var e in scr.AllElements)
                    Assert.IsTrue(e.IsAddressSet, $"{e} is not set address");
            }
        }

        [Test]
        public void RefsLifeCycle()
        {
            SCIPackage package = Utils.LoadPackage();

            foreach (var res in package.Scripts)
            {
                var scr = res.GetScript() as Script;
                scr.GetBytes();

                foreach (var r in scr.AllRefs)
                {
                    var descr = $"{res} {r.Address:x4} {r.Source}";
                    Assert.IsTrue(r.IsSetup, descr);
                    Assert.IsTrue(r.IsWrited, descr);
                    Assert.IsTrue(r.IsOffsetWrited, descr);
                }
            }
        }
    }
}