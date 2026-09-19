using System;
using System.Threading.Tasks;
using System.Linq;
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

        [TestCase(false)]
        [TestCase(true)]
        public void DecompileLoop(bool conditional)
        {
            var (decompiler, proc, node) = CreateTestGraph();
            node.NextA = new CodeNode { NextA = node };
            if (conditional)
            {
                node.Condition = new ParamExpr("keepGoing");
                node.NextB = new CodeNode { ReturnValue = new ConstExpr(42) };
            }

            var result = decompiler.DecompileProcedure(proc);

            Assert.That(result, Does.Contain("goto block_0;"));
            if (conditional)
                Assert.That(result, Does.Contain("return 42;"));
            Assert.That(decompiler.DecompileProcedure(proc), Is.EqualTo(result));
        }

        [Test]
        public void DecompileSelfLoop()
        {
            var (decompiler, proc, node) = CreateTestGraph();
            node.NextA = node;

            var result = decompiler.DecompileProcedure(proc);

            Assert.That(result, Does.Contain("block_0:;"));
            Assert.That(result, Does.Contain("goto block_0;"));
        }

        [Test]
        public void DecompileSharedReturnThroughEmptyBlock()
        {
            var (decompiler, proc, node) = CreateTestGraph();
            var returnNode = new CodeNode { ReturnValue = new ConstExpr(42) };
            node.Condition = new ParamExpr("chooseBranch");
            node.NextA = new CodeNode { NextA = returnNode };
            node.NextB = returnNode;

            var result = decompiler.DecompileProcedure(proc);

            Assert.That(result.Split("return 42;"), Has.Length.EqualTo(2));
            Assert.That(result, Does.Contain("else"));
            Assert.That(result, Does.Contain("block_2:;"));
            Assert.That(result, Does.Contain("goto block_2;"));
        }

        private static (ScriptDecompiler, ProcedureTree, CodeNode) CreateTestGraph()
        {
            var script = (Script)Utils.LoadBA().Scripts.First().GetScript();
            var decompiler = new ScriptDecompiler(script);
            var proc = decompiler.Procedures.First(p => p.Optimize().Node != null);
            var node = proc.Optimize().Node;
            node.NextA = null;
            node.NextB = null;
            node.Expressions = null;
            node.Condition = null;
            node.ReturnValue = null;
            return (decompiler, proc, node);
        }

        private void DecompilePackage(SCIPackage package)
        {
            Parallel.ForEach(package.Scripts, res =>
            {
                var script = res.GetScript();
                ClassicAssert.IsNotNull(script);

                var analyzer = script.Analyze();

                var graph = new GraphBuilder(analyzer);
                graph.GetGraph(GraphBuilder.CodeType.ASM);
                graph.GetGraph(GraphBuilder.CodeType.CPP);

                analyzer.Optimize();
                graph.GetGraph(GraphBuilder.CodeType.CPP_OPT);

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
                decompiler.Decompile();
            });
        }
    }
}
