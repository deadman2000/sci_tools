using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts1_1;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SCI_Tools
{
    // test -d D:\Dos\GAMES\QG_EGA\
    [Command("test", Description = "for testing")]
    class Test : PackageCommand
    {
        protected override Task Do()
        {
            try
            {
                /*foreach (ResScript rs in package.Scripts)
                {
                    var s = rs.GetScript() as Script1_1;
                    foreach (var obj in s.Objects)
                    {
                        if (obj.InstanceOf("Talker"))
                        {
                            var view = obj["view"];
                            Console.WriteLine($"{rs} {obj.Name} {view}");
                        }
                    }
                }*/

                /*{
                    var scripts = package.Scripts.Select(r => r.GetScript()).Cast<Script1_1>();

                    var talersInst = scripts.SelectMany(s => s.Objects).Where(o => o.InstanceOf("Talker")).OrderBy(t => t.ExportInd);
                    foreach (var t in talersInst)
                    {
                        Console.WriteLine($"{t.ExportInd}: {t}   {t.Script}");
                    }
                    Console.ReadKey();
                    Console.Clear();

                    var talkers = package.Messages.SelectMany(r => r.GetMessages()).Select(m => m.Talker).Where(t => t < 90).OrderBy(t => t).Distinct();
                    foreach (var t in talkers)
                    {
                        Console.WriteLine($"Talker {t}");
                        foreach (var s in scripts)
                        {
                            foreach (var obj in s.Objects)
                            {
                                if (obj.ExportInd.HasValue && obj.ExportInd.Value == t && obj.InstanceOf("Talker"))
                                    Console.WriteLine($"  {s.Resource}  {obj} : {obj.Super}");
                            }
                        }
                    }
                    Console.ReadKey();
                    Console.Clear();
                }*/

                /*var rmess = package.Messages.Select(r => new { r, m = r.GetMessages() }).Where(r => r.m.Any(m => m.Talker == 97)).Select(r => r.r);
                foreach (var r in rmess)
                {
                    Console.WriteLine(r);
                    foreach (var mess in r.GetMessages())
                    {
                        Console.WriteLine($"{mess.Talker}: {mess.Text}");
                    }
                }*/

                /*foreach (var scr in package.Scripts.Select(r => r.GetScript()).Cast<Script1_1>())
                {
                    Console.WriteLine(scr.Resource);
                    foreach (var obj in scr.Objects)
                    {
                        if (obj.ExportInd.HasValue && obj.ExportInd.Value > 0)
                            Console.WriteLine($"  {obj} export {obj.ExportInd}");
                    }
                }*/

                /*var res = package.Messages.Select(r => new { r, m = r.GetMessages() }).Where(r => r.m.Any(m => m.Talker < 90));
                foreach (var rm in res)
                {
                    Console.WriteLine($"Message {rm.r}");
                    var num = rm.r.Number;
                    var resScr = package.GetResource<ResScript>(num);
                    if (resScr != null)
                    {
                        Console.WriteLine("Has script");
                    }

                    var messages = rm.m;
                    var talkers = messages.Select(m => m.Talker).OrderBy(t => t).Distinct().Where(t => t < 90);
                    foreach (var t in talkers)
                    {
                        Console.WriteLine(t);
                    }
                }*/
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return Task.CompletedTask;
        }

        void PrintNouns()
        {
            var scripts = package.Scripts.Select(r => r.GetScript()).Cast<Script1_1>();

            foreach (var rm in package.Messages)
            {
                //var messages = rm.GetMessages().Where(m => m.Talker < 90);
                //if (!messages.Any()) continue;

                var resScr = package.GetResource<ResScript>(rm.Number);
                if (resScr == null)
                {
                    Console.WriteLine($"{rm}  No script ");
                    continue;
                }

                var scr = resScr.GetScript() as Script1_1;
                Console.WriteLine($"{rm} {scr.Objects[0]}");

                var nouns = scr.Objects.Select(o => new { o, noun = o.Properties.TryGetValue("noun", out var noun) ? noun.Value : 0 })
                    .Where(p => p.noun != 0)
                    .OrderBy(p => p.noun);
                foreach (var n in nouns)
                {
                    var view = n.o.Properties.TryGetValue("view", out var v) ? v.Value : -1;
                    var loop = n.o.Properties.TryGetValue("loop", out var l) ? l.Value : -1;
                    var cel = n.o.Properties.TryGetValue("cel", out var c) ? c.Value : -1;
                    if (view == -1)
                        Console.WriteLine($"  {n.noun} : {n.o} : {n.o.Super}");
                    else
                    {
                        Console.WriteLine($"  {n.noun} : {n.o} : {n.o.Super}   {view} - {loop} - {cel}");

                        var rv = package.GetResource<ResView>((ushort)view).GetView();
                        if (loop >= rv.Loops.Count)
                        {
                            Console.WriteLine("Wrong");
                        }
                        else
                        {
                            var rl = rv.Loops[loop];
                            if (cel >= rl.Cells.Count)
                            {
                                Console.WriteLine("Wrong");
                            }
                        }
                    }
                }

                /*foreach (var obj in scr.Objects)
                {
                    if (obj.Properties.TryGetValue("noun", out var noun) && noun.Value > 0)
                        Console.WriteLine($"  {obj} noun = {noun.Value}");
                }*/
            }
        }
    }
}
