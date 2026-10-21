using NUnit.Framework;
using SCI_Lib;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Builders;
using SCI_Lib.Resources.Scripts1;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tests
{
    public class ScriptBuilderTest
    {
        [Test]
        public void Sci11ListingsIncludeAllCodeAndHeapStrings()
        {
            CheckSci11Listings(Utils.LoadQG());
        }

        [Test]
        public void PepperSci11Listings()
        {
            const string directory = @"D:\Dos\GAMES\Peppers_Adventures_in_Time";
            if (!Directory.Exists(directory))
                Assert.Ignore("Игра Pepper's Adventures in Time не найдена");
            CheckSci11Listings(SCIPackage.Load(directory));
        }

        private static void CheckSci11Listings(SCIPackage package)
        {
            var asmBuilder = new SimpeScriptBuilder();
            var companionBuilder = new CompanionBuilder();
            var hexBuilder = new HexBuilder();

            foreach (var resource in package.Scripts)
            {
                var script = (Script1)resource.GetScript();
                var elements = script.AllElements.Concat(script.Heap.AllElements).ToArray();
                var addresses = elements.Select(element => element.Address).ToArray();
                var asm = asmBuilder.Decompile(script);
                var companion = companionBuilder.Decompile(script);
                var hex = hexBuilder.Decompile(script);

                var expected = script.Operators.Select(code => code.Address).ToArray();
                Assert.That(CodeAddresses(asm, @"^([0-9a-f]{4}): [0-9a-f]{2} "),
                    Is.EquivalentTo(expected), $"ASM {resource}");
                Assert.That(CodeAddresses(companion, @"^  ([0-9a-f]{4}):[0-9a-f]{2} "),
                    Is.EquivalentTo(expected), $"ASM Comp {resource}");

                foreach (var str in script.AllStrings())
                {
                    Assert.That(asm, Does.Contain($"string_{str.Address:x4} = '{str.ValueSlashEsc}'"), resource.ToString());
                    Assert.That(companion, Does.Contain($"string_{str.Address:x4} \"{str.ValueSlashEsc}\""), resource.ToString());
                }
                foreach (var obj in script.Objects)
                {
                    Assert.That(companion, Does.Contain($"({(obj.IsInstance ? "instance" : "class")} {obj.Name ?? obj.Label}"));
                    foreach (var method in obj.Methods)
                        Assert.That(companion, Does.Contain($"// method_{method.Address:x4}"));
                }

                Assert.That(hex, Does.StartWith($"Script {resource.Number} Size: {resource.GetContent().Length}"));
                Assert.That(hex, Does.Contain($"Heap {resource.Number} Size: {script.Heap.Resource.GetContent().Length}"));
                Assert.That(elements.Select(element => element.Address), Is.EqualTo(addresses), $"Listing changed addresses in {resource}");
            }
        }

        private static ushort[] CodeAddresses(string listing, string pattern) =>
            Regex.Matches(listing, pattern, RegexOptions.Multiline)
                .Select(match => Convert.ToUInt16(match.Groups[1].Value, 16)).Distinct().ToArray();

        [Test]
        public void Sci11CompanionKeepsEarlyReturnBranchesAndSeparatesLocalCalls()
        {
            var script = CreateScript(
            [
                28, 0, 0, 0, 0, 0, 2, 0, // relocation table and export count
                0, 0, 12, 0,             // empty export, code export
                0x35, 0,                  // 000c: ldi 0
                0x31, 1,                  // 000e: bnt 0011
                0x48,                     // 0010: ret (early return)
                0x72, 6, 0,               // 0011: lofsa heap string at 0006
                0x40, 1, 0, 0,            // 0014: call 0019
                0x48,                     // 0018: ret
                0x39, 0xff,                // 0019: pushi ff
                0x48,                     // 001b: ret
                1, 0, 18, 0               // relocation for lofsa operand
            ]);

            var companion = new CompanionBuilder().Decompile(script);
            Assert.That(companion, Does.Contain("    0 $0000"));
            Assert.That(companion, Does.Contain("    1 localproc_000c"));
            Assert.That(companion, Does.Contain("(procedure (localproc_0019)"));
            Assert.That(CodeAddresses(companion, @"^  ([0-9a-f]{4}):[0-9a-f]{2} "),
                Is.EquivalentTo(new ushort[] { 12, 14, 16, 17, 20, 24, 25, 27 }));
            Assert.That(new SimpeScriptBuilder().Decompile(script), Does.Contain("string_0006;  'X'"));

            // Viewing HEX after editing text must not relocate the live script/heap.
            script.AllStrings().First().Value = "A much longer string";
            var hex = new HexBuilder().Decompile(script);
            Assert.That(hex, Does.Contain("08 00 00 00 00 00 58 00 00 00"));
            Assert.That(script.AllStrings().First().Value, Is.EqualTo("A much longer string"));
            Assert.That(script.AllRefs.All(reference => !reference.IsWrited), Is.True);
        }

        [Test]
        public void Sci11ExportsDistinguishHeapAndCodeAtTheSameOffset()
        {
            var script = CreateScript(
            [
                20, 0, 0, 0, 0, 0, 2, 0, // header
                18, 0, 18, 0,            // object export and code export
                1, 0, 0, 0, 18, 0,       // one method at script:0012
                0x48, 0,                 // ret, padding
                1, 0, 8, 0               // only export 0 points to the heap
            ],
            [
                40, 0, 7, 0,             // string relocations, seven locals
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0x34, 0x12, 9, 0,        // object at heap:0012, nine properties
                12, 0, 12, 0, 0, 0,      // property/method table offsets
                0xff, 0xff, 0xff, 0xff, 0, 0, 38, 0,
                0, 0, (byte)'X', 0,       // end of objects and object name
                1, 0, 34, 0              // relocation of name property
            ]);

            var asm = new SimpeScriptBuilder().Decompile(script);
            var companion = new CompanionBuilder().Decompile(script);
            Assert.That(asm, Does.Contain("export_0 = X (0012)"));
            Assert.That(asm, Does.Contain("export_1 = localproc_0012 (0012)"));
            Assert.That(companion, Does.Contain("    0 X"));
            Assert.That(companion, Does.Contain("    1 localproc_0012"));
            Assert.That(companion, Does.Contain("(instance X"));
        }

        [Test]
        public void Sci11ObjectsWithoutNamePropertyCanBeDisplayed()
        {
            var script = CreateScript(
                [12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x48, 0, 0, 0],
                [22, 0, 0, 0, 0x34, 0x12, 8, 0, 8, 0, 8, 0, 0, 0,
                 0xff, 0xff, 0xff, 0xff, 0, 0, 0, 0, 0, 0]);

            Assert.That(script.Objects.Single().Name, Is.Null);
            Assert.That(script.Objects.Single().ToString(), Is.EqualTo("object1_0004"));
            Assert.That(new SimpeScriptBuilder().Decompile(script), Does.Contain("name = object1_0004"));
            Assert.That(new CompanionBuilder().Decompile(script), Does.Contain("(instance object1_0004"));
        }

        [Test]
        public void Sci11EmptyCodeAndRepeatedRendering()
        {
            var script = CreateScript([8, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
            var asmBuilder = new SimpeScriptBuilder();
            var companionBuilder = new CompanionBuilder();
            var hexBuilder = new HexBuilder();
            Assert.That(asmBuilder.Decompile(script), Is.EqualTo(asmBuilder.Decompile(script)));
            Assert.That(companionBuilder.Decompile(script), Is.EqualTo(companionBuilder.Decompile(script)));
            Assert.That(hexBuilder.Decompile(script), Is.EqualTo(hexBuilder.Decompile(script)));
        }

        private static Script1 CreateScript(byte[] data, byte[] heap = null)
        {
            var package = Utils.LoadQG();
            var resource = package.Scripts.First();
            resource.SetContent(data);
            package.GetResource<ResHeap>(resource.Number).SetContent(heap ?? [8, 0, 0, 0, 0, 0, (byte)'X', 0, 0, 0]);
            return (Script1)resource.GetScript();
        }

        [Test]
        public void SectionedScriptListingsStillRender()
        {
            var script = (Script)Utils.LoadEQ().Scripts.First().GetScript();
            var asmBuilder = new SimpeScriptBuilder();
            var companionBuilder = new CompanionBuilder();
            var hexBuilder = new HexBuilder();
            var asm = asmBuilder.Decompile(script);
            Assert.That(asmBuilder.Decompile(script), Is.EqualTo(asm));
            var companion = companionBuilder.Decompile(script);
            var hex = hexBuilder.Decompile(script);
            Assert.That(asm, Does.Contain("[Code section]"));
            Assert.That(companion, Does.Contain($"(script {script.Resource.Number})"));
            Assert.That(hex, Does.Contain("Type:"));
            Assert.That(companionBuilder.Decompile(script), Is.EqualTo(companion));
            Assert.That(hexBuilder.Decompile(script), Is.EqualTo(hex));
        }
    }
}
