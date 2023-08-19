using NUnit.Framework;
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
            Assert.AreEqual("$00", e.Escape("\0"));
            Assert.AreEqual("$01", e.Escape("\x0001"));
            Assert.AreEqual("\r", e.Escape("\r"));
            Assert.AreEqual("\n", e.Escape("\n"));
        }

        [Test]
        public void EscDollarFullTest()
        {
            var e = BaseEscaper.DollarFull;
            Assert.AreEqual("$00", e.Escape("\0"));
            Assert.AreEqual("$01", e.Escape("\x0001"));
            Assert.AreEqual("$0D", e.Escape("\r"));
            Assert.AreEqual("$0A", e.Escape("\n"));
        }

        [Test]
        public void EscSlashTest()
        {
            var e = BaseEscaper.Slash;
            Assert.AreEqual("\\0", e.Escape("\0"));
            Assert.AreEqual("\\x0001", e.Escape("\x0001"));
            Assert.AreEqual("\\r", e.Escape("\r"));
            Assert.AreEqual("\\n", e.Escape("\n"));
        }

        [Test]
        public void UnescSlashTest()
        {
            var e = BaseEscaper.Slash;
            var enc = Encoding.ASCII;
            Assert.AreEqual("\0", e.Unescape("\\0", enc));
            Assert.AreEqual("\x0001", e.Unescape("\\x0001", enc));
            Assert.AreEqual("\r", e.Unescape("\\r", enc));
            Assert.AreEqual("\n", e.Unescape("\\n", enc));
        }
    }
}
