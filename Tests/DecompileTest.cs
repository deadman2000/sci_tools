using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SCI_Lib;
using SCI_Lib.Analyzer;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts1;

namespace Tests
{
    public class DecompileTest
    {
        [Test]
        public void DecompileBA()
        {
            DecompilePackage(Utils.LoadBA());
        }

        private void DecompilePackage(SCIPackage package)
        {
            Parallel.ForEach(package.Scripts, res =>
            {
                var script = res.GetScript();
                ClassicAssert.IsNotNull(script);

                var analyzer = script.Analyze();
                var graph = new GraphBuilder(analyzer);

                analyzer.Optimize();

                ScriptDecompiler decompiler;

                if (script is Script script0)
                {
                    decompiler = new ScriptDecompiler(script0);

                }
                else if (script is Script1 script1)
                    decompiler = new ScriptDecompiler(script1);
                else
                    throw new NotImplementedException();

                decompiler.Optimize();
            });
        }
    }
}
