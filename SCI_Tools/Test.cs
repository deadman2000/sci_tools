using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Analyzer;
using SCI_Lib.Resources.Scripts.Builders;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Diagnostics;
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
                FindTextSaids();
                //DecompileAll();
                //Decompile(25);
                //Decompile(255, "DText");
                //Decompile(24, "Room24", "handleEvent");
                //Decompile(0, null, "handleEvent");

                /*HashSet<string> words = new();
                foreach (var res in translate.Scripts)
                {
                    var scr = res.GetScript() as Script;
                    var strings = scr.AllStrings().ToList();
                    foreach (var cs in scr.Get<CodeSection>())
                    {
                        foreach (var op in cs.Operators)
                        {
                            if (op.Name == "callb")
                            {
                                if ((byte)op.Arguments[0] == 0x19 && (byte)op.Arguments[1] == 2)
                                {
                                    var r = op.Prev.Prev.Arguments[0] as CodeRef;
                                    var s = r.Reference as StringConst;
                                    words.Add(s.Value);
                                    var ind = strings.IndexOf(s);
                                    Console.WriteLine($"{res.Number}:{ind}  {s.Value}");
                                }
                            }
                        }
                    }
                }

                foreach (var w in words.OrderBy(w => w))
                    Console.WriteLine(w);*/

                //var s = new SaidExtract(package);
                //s.Process(223);

                /*var resTxt = translate.GetResource<ResText>(208);
                var strings = resTxt.GetStrings();
                var result = new string[strings.Length];

                var res = translate.GetResource<ResScript>(208);
                var scr = res.GetScript() as Script;
                var vars = scr.Get<LocalVariablesSection>()[0].Vars;

                for (int verb = 0; verb <= 10; verb++)
                {
                    var from = (ushort)vars[150 + verb * 2];
                    var to = from + (ushort)vars[150 + verb * 2 + 1];
                    for (int noun = from; noun < to; noun++)
                    {
                        var txtRes = (ushort)vars[1 + noun * 2];
                        var txtInd = (ushort)vars[1 + noun * 2 + 1];
                        var v = ((SaidExpression)((RefToElement)vars[93 + verb]).Reference).Label.TrimEnd('>');
                        Console.WriteLine($"{noun}  {v}{vars[104 + noun]}   {txtRes}:{txtInd}");
                    }
                }*/


                //FindTextCall(374, 3);

                //FindWordUsage(0x430);

                /*PrintHeap(260);
                SetHeap(260, 12, "Бифф");
                SetHeap(260, 13, "Вонючка");
                SetHeap(260, 14, "Пузатый");
                SetHeap(1902, 0, "Рокко");
                SetHeap(1903, 0, "Боб");*/

                {
                    /*var voc = package.GetResource<ResVocab>(0) as ResVocab000;
                    var words = voc.GetWords();
                    var groups = words.GroupBy(w => w.Group);
                    foreach (var gr in groups.OrderBy(g => g.Key))
                    {
                        foreach (var clGr in gr.GroupBy(g => g.Class).OrderBy(g => g.Key))
                        {
                            var groupWords = string.Join(',', gr.Select(w => w.Text));
                            Console.WriteLine($"{clGr.Key:X03} {gr.Key:X03} {groupWords}");
                        }
                    }*/
                }

                {
                    /*var voc = translate.GetResource(ResType.Vocabulary, 1) as ResVocab001;
                    var words = voc.GetWords();
                    foreach (var w in words)
                        Console.WriteLine($"{w.Class:X03} {w.Group:X03} {w}");*/

                    /*voc.SetWords(new Word[] {
                        new Word("осмотрись", 0x3E8, 0x800)
                    });
                    voc.SavePatch();*/
                }

                /*{
                    File.Delete(@"D:\Dos\GAMES\Laura_Bow_RUS\vocab.001");
                    var voc = translate.CreateResource(ResType.Vocabulary, 1) as ResVocab001;
                    List<Word> words = new();

                    var lines = File.ReadAllLines(@"c:\Projects\TranslateWeb\words.txt");
                    foreach (var line in lines)
                    {
                        var parts = line.Split(' ');
                        var id = int.Parse(parts[0], System.Globalization.NumberStyles.HexNumber);
                        ushort cl = (ushort)(id >> 12);
                        ushort gr = (ushort)(id & 0xfff);
                        var wordsStr = parts[1].Split(',');
                     
                        foreach (var w in wordsStr)
                            words.Add(new Word(w, cl, gr));
                    }

                    voc.SetWords(words.ToArray());
                    voc.SavePatch();
                }*/

                /*CopyOriginalFont(0);
                CopyOriginalFont(1);
                CopyOriginalFont(4);*/
                //CopyOriginalFont(103);
                //GenerateOutline(103, 104);

                //PatchFont();

                // LB2 Fix
                //FixRLE(527);
                //FixRLE(456);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return Task.CompletedTask;
        }

        private void FindTextSaids(ushort? scr = null)
        {
            var search = new TextUsageSearch(package, scr);
            var result = search.FindUsage();

            foreach (var p in result.OrderBy(p => p.Script).ThenBy(p => p.Index))
            {
                Console.WriteLine($"{p.Script}.{p.Index}");
                foreach (var s in p.Saids)
                    Console.WriteLine($"\t{s}");
            }
        }

        private void Decompile(ushort num, string cl = null, string method = null)
        {
            var res = package.GetResource<ResScript>(num);
            var script = res.GetScript() as Script;

            var analyzer = script.Analyze(cl, method);
            var graph = new GraphBuilder(analyzer);

            CreateGraph(res.Number, graph, GraphBuilder.CodeType.CPP);
            CreateGraph(res.Number, graph, GraphBuilder.CodeType.Meta);
            //CreateGraph(res.Number, graph, GraphBuilder.CodeType.ASM);

            analyzer.Optimize();
            CreateGraph(res.Number, graph, GraphBuilder.CodeType.CPP_OPT);

            var h_path = @$"c:\Projects\TranslateWeb\out\scr{num}.h";
            File.WriteAllText(h_path, new CppBuilder(cl, method).Decompile(script));
        }

        private void CreateGraph(ushort number, GraphBuilder graph, GraphBuilder.CodeType type)
        {
            var dot_path = @$"c:\Projects\TranslateWeb\out\{number}_{type.ToString().ToLower()}.graph";
            var svg_path = @$"c:\Projects\TranslateWeb\out\{number}_{type.ToString().ToLower()}.svg";
            File.WriteAllText(dot_path, graph.GetGraph(type));
            Debug.WriteLine("Start dot");
            var proc = new Process();
            proc.StartInfo = new ProcessStartInfo("dot", $"-Tsvg {dot_path} -o {svg_path}");
            proc.OutputDataReceived += Proc_OutputDataReceived;
            proc.Start();
            proc.WaitForExit();
            Debug.WriteLine($"dot result: {proc.ExitCode}");
            if (proc.ExitCode == 0)
                File.Delete(dot_path);
        }

        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void DecompileAll()
        {
            foreach (var res in package.GetResources<ResScript>())
            {
                Console.WriteLine(res);
                var script = res.GetScript() as Script;
                var analyzer = script.Analyze();
                var graph = new GraphBuilder(analyzer);
                graph.GetGraph(GraphBuilder.CodeType.CPP);
            }
        }

        private void FindTextCall(int text, int index)
        {
            var resources = package.Scripts.GroupBy(r => r.Number).Select(g => g.First());

            foreach (var res in resources)
            {
                var scr = res.GetScript() as Script;

                foreach (var codeSection in scr.Get<CodeSection>())
                {
                    for (int i = 0; i < codeSection.Operators.Count - 2; i++)
                    {
                        var op = codeSection.Operators[i];
                        if (op.Name == "pushi" && ToInt(op.Arguments[0]) == text)
                        {
                            var op2 = codeSection.Operators[i + 1];
                            if (op2.Name == "pushi" && ToInt(op2.Arguments[0]) == index)
                            {
                                var op3 = codeSection.Operators[i + 2];
                                if (op3.Name == "calle" && op3.ArgsToString() == "$ff, $00, $04")
                                {
                                    Console.WriteLine($"Found in {res.Number} {op.Address:x04}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private static int ToInt(object obj)
        {
            if (obj is byte b) return b;
            if (obj is ushort s) return s;
            return 0;
        }

        private void FindWordUsage(ushort word)
        {
            var resources = package.Scripts;
            var scripts = resources
                .GroupBy(r => r.Number).Select(g => g.First())
                .Select(r => r.GetScript() as Script).ToList();

            var wordsUsage = scripts
                .Select(s => new
                {
                    Script = s,
                    Words = s.Get<SaidSection>().SelectMany(ss => ss.Saids)
                        .SelectMany(s => s.Expression)
                        .Where(e => !e.IsOperator)
                        .Select(s => s.Data)
                        .Union(s.Get<SynonymSecion>().SelectMany(s => s.Synonyms).Select(s => s.WordA))
                        .Distinct()
                        .ToHashSet()
                });

            foreach (var s in wordsUsage)
            {
                if (s.Words.Contains(word))
                    Console.WriteLine(s.Script.Resource.Number);
            }

        }

        private void PrintHeap(ushort num)
        {
            var res = translate.GetResource<ResHeap>(num);
            var strings = res.GetStrings();
            for (int i = 0; i < strings.Length; i++)
                Console.WriteLine($"[{i}] {strings[i]}");
            Console.WriteLine();
        }

        private void SetHeap(ushort num, int ind, string value)
        {
            var res = translate.GetResource<ResHeap>(num);
            var strings = res.GetStrings();
            strings[ind] = value;
            res.SavePatch();
        }

        private void CopyOriginalFont(ushort num)
        {
            // Заменяет символы в переводном шрифте на оригинальные
            var srcRes = package.GetResource<ResFont>(num);
            var trRes = translate.GetResource<ResFont>(num);
            var font = srcRes.GetFont();
            var trFont = trRes.GetFont();

            for (int i = 0; i < font.Frames.Count; i++)
                trFont[i] = font[i];

            trRes.SetFont(trFont);
            trRes.SavePatch();
        }

        private void PatchFont()
        {
            // Добавляет пространство в 1 пиксель справа и сдвигет шрифт вниз
            // Camelot 103
            var res = translate.GetResource<ResFont>(103);
            var fnt = res.GetFont();
            for (int i = 0x80; i < fnt.Frames.Count; i++)
            {
                var f = fnt.Frames[i];
                if (f.Width == 1) continue;
                f.Resize(f.Width + 1, f.Height + 1);
                f.ShiftDown(0);
            }

            res.SetFont(fnt);
            res.SavePatch();
        }

        private void GenerateOutline(ushort src, ushort dst)
        {
            // Генерирует обводный шрифт
            // Camelot 103->104
            var res = translate.GetResource<ResFont>(src);
            var fntOutline = translate.GetResource<ResFont>(dst);
            fntOutline.GenerateOutline(res, 0x80);
            fntOutline.SavePatch();
        }

        void FixRLE(ushort num)
        {
            var res = translate.GetResource<ResView>(num);
            var view = res.GetView();
            view.Loops[0].Cells[0].NoRLE = true;
            res.SetView(view);
            res.SavePatch();
        }

        void ExtractText()
        {
            using var file = File.CreateText(@"c:\Projects\TranslateWeb\ru_text.txt");
            var messages = package.GetResources<ResMessage>();
            foreach (var msg in messages)
            {
                file.WriteLine($"{msg.Number}.msg");
                foreach (var str in msg.GetStrings())
                {
                    file.WriteLine(str);
                }
                file.WriteLine();
            }
        }

        void RobinPatch851()
        {
            var res = package.GetResource<ResScript>(851);
            var scr = res.GetScript();

            // Патчим строку
            var stringsSec = scr.Get<StringSection>().First();
            stringsSec.Strings[8].Value = "Aye";

            // Фиксим оператор
            var code = scr.Get<CodeSection>().ElementAt(4);
            var op = code.Operators.Find(o => o.Address == 0xb9f);
            op.Arguments[0] = (ushort)0x10ac;

            res.SavePatch();
        }
    }
}
