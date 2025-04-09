using NUnit.Framework;
using SCI_Lib;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Resources.Scripts1;
using System;
using System.Linq;

namespace Tests
{
    public class ScriptsTest
    {
        [SetUp]
        public void Setup()
        {
        }

        private static void CheckPackage(SCIPackage package)
        {
            foreach (var res in package.Scripts)
            {
                var scr = res.GetScript();
                Assert.IsNotNull(scr);

                if (scr is Script s0)
                    CheckScriptValid(s0);
                else if (scr is Script1 s1)
                    CheckScriptValid(s1);
                else
                    throw new NotImplementedException();
            }
        }

        private static void CheckScriptValid(Script scr)
        {
            // Проверка ссылок на ссылки
            foreach (var rel in scr.Sections.OfType<RelocationSection>())
            {
                foreach (var r in rel.Refs)
                {
                    var descr = $"Res: {scr.Resource} Ref at {r.Address:x4} Target: {r.TargetOffset:x4}";
                    Assert.IsNotNull(r.Reference, descr);
                    if (r.Reference is not BaseRef && r.Reference is not PropertyElement)
                        Assert.Fail(descr);
                }
            }
        }

        private static void CheckScriptValid(Script1 scr)
        {
            foreach (var obj in scr.Objects)
            {
                foreach (var m in obj.Methods)
                {
                    Assert.NotNull(m.Reference.Reference, $"Wrong method reference in {scr.Resource.Number} {m.Name}");
                }
            }
        }

        [Test]
        public void DisassembleConquest()
        {
            CheckPackage(Utils.LoadConquest());
        }

        [Test]
        public void DisassembleEQ()
        {
            CheckPackage(Utils.LoadEQ());
        }

        [Test]
        public void DisassembleQG()
        {
            CheckPackage(Utils.LoadQG());
        }

        [Test]
        public void ParseAndBackConquest() => ParseAndBack(Utils.LoadConquest());

        [Test]
        public void ParseAndBackEQ() => ParseAndBack(Utils.LoadEQ());

        [Test]
        public void ParseAndBackQG() => ParseAndBack1(Utils.LoadQG());

        private static void ParseAndBack(SCIPackage package)
        {
            foreach (var res in package.Scripts)
            {
                var bytes = res.GetContent();
                var newbytes = res.GetScript().GetBytes();
                CollectionAssert.AreEqual(bytes, newbytes, $"Problem in {res.Number}");
            }
        }

        private static void ParseAndBack1(SCIPackage package)
        {
            foreach (var res in package.Scripts)
            {
                var resHeap = package.GetResource<ResHeap>(res.Number);

                var bytes = res.GetContent();
                var bytesH = resHeap.GetContent();

                var newbytes = res.GetScript().GetBytes();
                var newbytesH = resHeap.GetHeap().GetBytes();

                CollectionAssert.AreEqual(bytes, newbytes, $"Problem in script {res.Number}");
                CollectionAssert.AreEqual(bytesH, newbytesH, $"Problem in heap {res.Number}");
            }
        }

        [Test]
        public void SetAddressOnWrite()
        {
            SCIPackage package = Utils.LoadConquest();

            foreach (var r in package.Scripts)
            {
                var scr = r.GetScript() as Script;
                foreach (var e in scr.AllElements)
                    e.ResetAddress();

                scr.GetBytes();

                foreach (var e in scr.AllElements)
                    Assert.IsTrue(e.Address != 0, $"{e} is not set address");
            }
        }

        [Test]
        public void RefsLifeCycle()
        {
            SCIPackage package = Utils.LoadConquest();

            foreach (var res in package.Scripts)
            {
                var scr = res.GetScript() as Script;
                scr.GetBytes();

                foreach (var r in scr.AllRefs)
                {
                    var descr = $"{res} {r.Address:x4}";
                    Assert.IsTrue(r.IsSetup, descr);
                    Assert.IsTrue(r.IsWrited, descr);
                    Assert.IsTrue(r.IsOffsetWrited, descr);
                }
            }
        }

        [Test]
        public void StringsRefs()
        {
            // Проверяем, теряются ли имена классов при сдвиге адресов строк
            SCIPackage package = Utils.LoadConquest();
            var res = package.GetResource<ResScript>(0);
            var scr = res.GetScript() as Script;

            // Запоминаем имена классов
            var names = scr.Get<ClassSection>().Select(c => c.Name).ToArray();

            // Меняем размер первой строки. Все следующие строки сдвинутся на 1 байт
            var str = scr.StringSection.Strings[0];
            var oldValue = str.Value;
            str.Value += "1";

            var data = res.GetPatch();
            var newScr = new Script(res, data);
            var newNames = newScr.Get<ClassSection>().Select(c => c.Name).ToArray();

            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == oldValue) continue;
                Assert.AreEqual(names[i], newNames[i]);
            }
        }
    }
}