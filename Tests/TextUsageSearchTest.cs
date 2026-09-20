using NUnit.Framework;
using SCI_Lib.Analyzer;
using System.Linq;
namespace Tests;
public class TextUsageSearchTest
{
    [TestCase((ushort)35)]
    [TestCase((ushort)142)]
    public void HandlesSaidWithoutArguments(ushort script)
    {
        Assert.DoesNotThrow(() => new TextUsageSearch(Utils.LoadBA(), script).FindUsage().ToArray());
    }

    [TestCase(75, "get/(flask<ye)")]
    [TestCase(74, "/tapestry")]
    [TestCase(27, "/d")]
    public void FindsBetrayedAllianceText(int index, string said)
    {
        var usage = new TextUsageSearch(Utils.LoadBA(), 18).FindUsage().ToArray();
        var print = usage.Single(p => p.Txt == 18 && p.Index == index);
        Assert.That(print.Saids.Select(s => s.Label), Does.Contain(said));
        if (index == 27)
            Assert.That(print.Saids.Select(s => s.Label), Does.Contain("(see,examine,read)>"));
    }

    [Test]
    public void FindsBetrayedAllianceTextAcrossPackage()
    {
        var usage = new TextUsageSearch(Utils.LoadBA()).FindUsage().ToArray();
        Assert.That(usage.Any(p => p.Txt == 18 && p.Index == 75 &&
            p.Saids.Any(s => s.Label == "get/(flask<ye)")), Is.True);
    }

}

