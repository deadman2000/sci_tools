using NUnit.Framework;
using NUnit.Framework.Legacy;
using SCI_Lib;
using System.Text;

namespace Tests
{
    class StringTest
    {
        [Test]
        public void EscDollarTest()
        {
            var e = BaseEscaper.Dollar;
            ClassicAssert.AreEqual("$00", e.Escape("\0"));
            ClassicAssert.AreEqual("$01", e.Escape("\x0001"));
            ClassicAssert.AreEqual("\r", e.Escape("\r"));
            ClassicAssert.AreEqual("\n", e.Escape("\n"));
        }

        [Test]
        public void EscDollarFullTest()
        {
            var e = BaseEscaper.DollarFull;
            ClassicAssert.AreEqual("$00", e.Escape("\0"));
            ClassicAssert.AreEqual("$01", e.Escape("\x0001"));
            ClassicAssert.AreEqual("$0D", e.Escape("\r"));
            ClassicAssert.AreEqual("$0A", e.Escape("\n"));
        }

        [Test]
        public void EscSlashTest()
        {
            var e = BaseEscaper.Slash;
            ClassicAssert.AreEqual("\\0", e.Escape("\0"));
            ClassicAssert.AreEqual("\\x0001", e.Escape("\x0001"));
            ClassicAssert.AreEqual("\\r", e.Escape("\r"));
            ClassicAssert.AreEqual("\\n", e.Escape("\n"));
        }

        [Test]
        public void UnescSlashTest()
        {
            var e = BaseEscaper.Slash;
            var enc = Encoding.ASCII;
            ClassicAssert.AreEqual("\0", e.Unescape("\\0", enc));
            ClassicAssert.AreEqual("\x0001", e.Unescape("\\x0001", enc));
            ClassicAssert.AreEqual("\r", e.Unescape("\\r", enc));
            ClassicAssert.AreEqual("\n", e.Unescape("\\n", enc));
        }
    }
}
