using NUnit.Framework;
using NUnit.Framework.Legacy;
using SCI_Lib;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Resources.Scripts1;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            Parallel.ForEach(package.Scripts, res =>
            {
                var scr = res.GetScript();
                ClassicAssert.IsNotNull(scr);

                if (scr is Script s0)
                    CheckScriptValid(s0);
                else if (scr is Script1 s1)
                    CheckScriptValid(s1);
                else
                    throw new NotImplementedException();
            });
        }

        private static void CheckScriptValid(Script scr)
        {
            // �������� ������ �� ������
            Parallel.ForEach(scr.Sections.OfType<RelocationSection>(), rel =>
            {
                foreach (var r in rel.Refs)
                {
                    var descr = $"Res: {scr.Resource} Ref at {r.Address:x4} Target: {r.TargetOffset:x4}";
                    ClassicAssert.IsNotNull(r.Reference, descr);
                    if (r.Reference is not BaseRef && r.Reference is not PropertyElement)
                        Assert.Fail(descr);
                }
            });
        }

        private static void CheckScriptValid(Script1 scr)
        {
            Parallel.ForEach(scr.Objects, obj =>
            {
                foreach (var m in obj.Methods)
                {
                    ClassicAssert.NotNull(m.Reference.Reference, $"Wrong method reference in {scr.Resource.Number} {m.Name}");
                }
            });
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
            Parallel.ForEach(package.Scripts, res =>
            {
                var bytes = res.GetContent();
                var newbytes = res.GetScript().GetBytes();
                CollectionAssert.AreEqual(bytes, newbytes, $"Problem in {res.Number}");
            });
        }

        private static void ParseAndBack1(SCIPackage package)
        {
            Parallel.ForEach(package.Scripts, res =>
            {
                var resHeap = package.GetResource<ResHeap>(res.Number);

                var bytes = res.GetContent();
                var bytesH = resHeap.GetContent();

                var newbytes = res.GetScript().GetBytes();
                var newbytesH = resHeap.GetHeap().GetBytes();

                CollectionAssert.AreEqual(bytes, newbytes, $"Problem in script {res.Number}");
                CollectionAssert.AreEqual(bytesH, newbytesH, $"Problem in heap {res.Number}");
            });
        }

        [Test]
        public void SetAddressOnWrite()
        {
            SCIPackage package = Utils.LoadConquest();

            Parallel.ForEach(package.Scripts, r =>
            {
                var scr = r.GetScript() as Script;
                foreach (var e in scr.AllElements)
                    e.ResetAddress();

                scr.GetBytes();

                foreach (var e in scr.AllElements)
                    ClassicAssert.IsTrue(e.Address != 0, $"{e} is not set address");
            });
        }

        [Test]
        public void RefsLifeCycle()
        {
            SCIPackage package = Utils.LoadConquest();

            Parallel.ForEach(package.Scripts, res =>
            {
                var scr = res.GetScript() as Script;
                scr.GetBytes();

                foreach (var r in scr.AllRefs)
                {
                    var descr = $"{res} {r.Address:x4}";
                    ClassicAssert.IsTrue(r.IsSetup, descr);
                    ClassicAssert.IsTrue(r.IsWrited, descr);
                    ClassicAssert.IsTrue(r.IsOffsetWrited, descr);
                }
            });
        }

        [Test]
        public void StringsRefs()
        {
            // ���������, �������� �� ����� ������� ��� ������ ������� �����
            SCIPackage package = Utils.LoadConquest();
            var res = package.GetResource<ResScript>(0);
            var scr = res.GetScript() as Script;

            // ���������� ����� �������
            var names = scr.Get<ClassSection>().Select(c => c.Name).ToArray();

            // ������ ������ ������ ������. ��� ��������� ������ ��������� �� 1 ����
            var str = scr.StringSection.Strings[0];
            var oldValue = str.Value;
            str.Value += "1";

            var data = res.GetPatch();
            var newScr = new Script(res, data);
            var newNames = newScr.Get<ClassSection>().Select(c => c.Name).ToArray();

            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == oldValue) continue;
                ClassicAssert.AreEqual(names[i], newNames[i]);
            }
        }
    }
}