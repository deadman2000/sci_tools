using McMaster.Extensions.CommandLineUtils;
using Nest;
using SCI_Lib;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Builders;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Resources.Scripts1_1;
using SCI_Lib.Resources.Vocab;
using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
                //var s = new SaidExtract(package);
                //s.Process(291);

                /*CBSkipButtonPatch(37, 101);
                CBSkipButtonPatch(301);
                CBSkipButtonPatch(302);
                CBSkipButtonPatch(303);
                CBSkipButtonPatch(305);
                CBSkipButtonPatch(321);
                CBSkipButtonPatch(323);
                CBSkipButtonPatch(330);
                CBSkipButtonPatch(333);
                CBSkipButtonPatch(350);
                CBSkipButtonPatch(353);
                CBSkipButtonPatch(354);*/

                //PrintSaids(17);
                //PatchSaid(3, 19, "потяни,позвони/трость<(кольцо,веревка,колокол)");
                //PatchSaid(3, 20, "потяни,позвони/(кольцо,веревка,колокол)<трость");
                //PatchSaid(7, 1, "<под/мост");
                //PatchSaid(7, 10, "осмотри<мост");
                //PatchSaid(10, 5, "<up");
                //PatchSaid(12, 5, "<up");
                //PatchSaid(12, 7, "постучи<в/погреб<дверцу");
                //AddSynonym(14, "домик", "дом");
                //PatchSaid(15, 4, "<up");
                //PatchSaid(16, 3, "<down");
                //PatchSaid(16, 4, "<up");
                //PatchSaid(17, 3, "<up");
                //AddSynonym(65, "ваза", "урна");



                //FixRLE(527);
                //FixRLE(456);

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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return Task.CompletedTask;
        }

        private void CBSkipButtonPatch(ushort scriptNum, byte srcKey = 0x73)
        {
            // script.301   036a:35 73              ldi 73
            var res = translate.GetResource<ResScript>(scriptNum);
            var scr = res.GetScript() as Script;
            bool found = false;
            foreach (var codeSec in scr.Get<CodeSection>())
            {
                var op = codeSec.Operators.Find(o => o.Name == "ldi" && (byte)o.Arguments[0] == srcKey);
                if (op != null)
                {
                    op.Arguments[0] = (byte)32;
                    found = true;
                    break;
                }
            }
            if (found)
                res.SavePatch();
            else
                Console.WriteLine($"{scriptNum} Operator not found");
        }

        private void AddSynonym(ushort scriptNum, string w1, string w2)
        {
            var res = translate.GetResource<ResScript>(scriptNum);
            var scr = res.GetScript() as Script;
            SynonymSecion section;

            var sections = scr.Get<SynonymSecion>();
            if (sections.Count == 0)
                section = scr.CreateSection(SectionType.Synonym) as SynonymSecion;
            else
                section = sections[0];
            var w1Ids = translate.GetWordId(w1);
            if (w1Ids == null || w1Ids.Length > 1) throw new Exception();
            var w2Ids = translate.GetWordId(w2);
            if (w2Ids == null || w2Ids.Length > 1) throw new Exception();

            var id1 = w1Ids[0];
            var id2 = w2Ids[0];

            if (section.Synonyms.Exists(s => (s.WordA == id1 && s.WordB == id2) || (s.WordA == id2 && s.WordB == id1)))
                return;

            section.Synonyms.Add(new Synonym
            {
                WordA = w1Ids[0],
                WordB = w2Ids[0]
            });

            res.SavePatch();
        }

        private void PrintSaids(ushort scriptNum)
        {
            var res = translate.GetResource<ResScript>(scriptNum);
            var scr = res.GetScript() as Script;
            var saidSection = scr.Get<SaidSection>()[0];
            for (int i = 0; i < saidSection.Saids.Count; i++)
            {
                var said = saidSection.Saids[i];
                Console.WriteLine($"{i} = {said}     {said.Hex}");
            }
        }

        private void PatchSaid(ushort scriptNum, int ind, string str)
        {
            var res = translate.GetResource<ResScript>(scriptNum);
            var scr = res.GetScript() as Script;
            var saidSection = scr.Get<SaidSection>()[0];
            saidSection.Saids[ind].Set(str);
            res.SavePatch();
        }

        private void FindTextCall(int text, int index)
        {
            var resources = package.GetResources<ResScript>().GroupBy(r => r.Number).Select(g => g.First());

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
            var resources = package.GetResources<ResScript>();
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
            var srcRes = package.GetResource<ResFont>(num);
            var trRes = translate.GetResource<ResFont>(num);
            var font = srcRes.GetFont();
            var trFont = trRes.GetFont();

            for (int i = 0; i < font.Frames.Count; i++)
                trFont[i] = font[i];

            trRes.SetFont(trFont);
            trRes.SavePatch();
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
            var stringsSec = scr.Get<StringSection>()[0];
            stringsSec.Strings[8].Value = "Aye";

            // Фиксим оператор
            var code = scr.Get<CodeSection>()[4];
            var op = code.Operators.Find(o => o.Address == 0xb9f);
            op.Arguments[0] = (ushort)0x10ac;

            res.SavePatch();
        }
    }
}
