using McMaster.Extensions.CommandLineUtils;
using SCI_Lib;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Resources.Scripts1;
using SCI_Lib.Resources.Vocab;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SCI_Tools
{
    abstract class PatchCommand : BaseCommand
    {
        [Option(Description = "Game directory", ShortName = "d", LongName = "dir")]
        [Required]
        public string GameDir { get; set; }

        private readonly HashSet<Resource> _changed = new();
        protected SCIPackage _translate;
        private HashSet<ushort> _groups;
        protected Dictionary<ushort, ushort[]> _wordsUsage;

        protected override Task Execute()
        {
            _translate = SCIPackage.Load(GameDir);
            Patch();
            Save();
            return Task.CompletedTask;
        }

        protected abstract void Patch();

        protected void Changed(Resource res)
        {
            //if (res is ResScript && _translate.SeparateHeapResources)
                //_changed.Add(_translate.GetResource<ResHeap>(res.Number));
            _changed.Add(res);
        }

        protected void Save()
        {
            foreach (var res in _changed)
            {
                Console.WriteLine($"Changed: {res}");
                res.SavePatch();
            }
            _changed.Clear();
            _wordsUsage = null;
        }

        protected void Commit()
        {
            if (_changed.Any())
            {
                Save();
                _translate = SCIPackage.Load(GameDir);
            }
        }

        #region Words

        private ushort NextWordGroup()
        {
            _groups ??= _translate.GetIdToWord().Select(kv => kv.Key).ToHashSet();

            for (ushort i = 1; i < 0xeff; i++)
            {
                if (!_groups.Contains(i))
                {
                    _groups.Add(i);
                    return i;
                }
            }
            return ushort.MaxValue;
        }

        protected void CreateWord(string newWords, WordClass cl)
        {
            var res = (ResVocab001)_translate.GetResource(ResType.Vocabulary, 1);
            var words = new List<Word>(res.GetWords());

            var wordsArr = newWords.Split(',');
            ushort group = 0;
            foreach (var word in wordsArr)
            {
                var exists = words.FirstOrDefault(w => w.Text == word && w.Class == cl);
                if (exists != null)
                {
                    group = exists.Group;
                    break;
                }
            }

            if (group == 0)
                group = NextWordGroup();

            bool changed = false;
            foreach (var word in wordsArr)
            {
                if (!words.Any(w => w.Text == word && w.Class == cl))
                {
                    words.Add(new Word(word, cl, group));
                    changed = true;
                }
            }

            res.SetWords(words);

            if (changed)
                Changed(res);
        }

        protected void FindRuDuplicate()
        {
            var words = _translate.GetWordIds();
            var list = words.Where(kv => kv.Value.Length > 1)
                .Select(kv => kv.Key)
                .Where(w => w[0] >= 'а' && w[0] <= 'я');

            foreach (var word in list)
            {
                Console.WriteLine(word);
            }
        }

        protected void RemoveSaidDubl()
        {
            var resources = _translate.Scripts
                .GroupBy(r => r.Number)
                .Select(g => g.First());

            foreach (var res in resources)
            {
                var scr = res.GetScript() as Script;
                var ss = scr.SaidSection;
                if (ss == null) continue;

                foreach (var said in ss.Saids)
                {
                    if (said.Normalize())
                    {
                        //Console.WriteLine($"{before} => {said.Label}");
                        Changed(res);
                    }
                }
            }
        }

        protected void BuildWordUsageMap()
        {
            if (_wordsUsage != null) return;

            var resources = _translate.Scripts
                .GroupBy(r => r.Number).Select(g => g.First());

            var scripts = resources.Select(r => r.GetScript() as Script)
                .Where(s => s != null)
                .ToList();

            _wordsUsage = scripts
                .SelectMany(s => s.Get<SaidSection>().SelectMany(ss => ss.Saids)
                        .SelectMany(s => s.Expression)
                        .Where(e => !e.IsOperator)
                        .Select(s => s.Data)
                        .Union(s.Get<SynonymSecion>().SelectMany(s => s.Synonyms).Select(s => s.WordA))
                        .Distinct()
                        .Select(w => new { S = s, W = w })
                )
                .GroupBy(i => i.W)
                .ToDictionary(g => g.Key, g => g.Select(n => n.S.Resource.Number).ToArray());
        }

        protected void ReplaceWord(string wordFrom, string wordTo, params ushort[] scripts)
        {
            var from = _translate.GetWordId(wordFrom)[0];
            var to = _translate.GetWordId(wordTo)[0];

            foreach (var scriptNum in scripts)
                ReplaceWord(scriptNum, from, to);
        }

        protected void ReplaceWord(string wordFrom, string wordTo)
        {
            var from = _translate.GetWordId(wordFrom)[0];
            var to = _translate.GetWordId(wordTo)[0];

            BuildWordUsageMap();

            if (_wordsUsage.TryGetValue(from, out var nums))
                foreach (var scriptNum in nums)
                    ReplaceWord(scriptNum, from, to);
        }

        protected void ReplaceWord(ushort scriptNum, ushort from, ushort to)
        {
            var res = _translate.GetResource<ResScript>(scriptNum);
            var scr = res.GetScript() as Script;

            bool changed = false;
            foreach (var saidSec in scr.Get<SaidSection>())
            {
                foreach (var said in saidSec.Saids)
                {
                    foreach (var e in said.Expression)
                    {
                        if (!e.IsOperator && e.Data == from)
                        {
                            e.Data = to;
                            changed = true;
                        }
                    }
                }

                foreach (var synSec in scr.Get<SynonymSecion>())
                {
                    for (int i = 0; i < synSec.Synonyms.Count; i++)
                    {
                        var syn = synSec.Synonyms[i];

                        if (syn.WordA == from)
                        {
                            if (syn.WordB == to || syn.WordA == syn.WordB)
                            {
                                synSec.Synonyms.RemoveAt(i);
                                i--;
                                changed = true;
                            }
                            else if (syn.WordB != to)
                            {
                                synSec.Synonyms[i] = new Synonym
                                {
                                    WordA = to,
                                    WordB = syn.WordB
                                };
                                changed = true;
                            }
                        }
                        else if (syn.WordB == from)
                        {
                            if (syn.WordA == to || syn.WordA == syn.WordB)
                            {
                                synSec.Synonyms.RemoveAt(i);
                                i--;
                                changed = true;
                            }
                            else if (syn.WordA != to)
                            {
                                synSec.Synonyms[i] = new Synonym
                                {
                                    WordA = syn.WordA,
                                    WordB = to
                                };
                                changed = true;
                            }
                        }
                    }
                }
            }

            if (changed)
                Changed(res);
        }

        protected void PrintEnWords()
        {
            BuildWordUsageMap();
            foreach (var id in _wordsUsage.Keys)
            {
                var word = _translate.GetIdToWord()[id];
                if (word[0] >= 'a' && word[0] <= 'z')
                    Console.WriteLine(word);
            }
        }

        protected void PrintAllScripts()
        {
            BuildWordUsageMap();
            var scripts = _wordsUsage.Values.SelectMany(v => v).Distinct().OrderBy(v => v);
            foreach (var scr in scripts)
                Console.WriteLine($"{scr,-4:D03}_");
        }

        #endregion

        #region Synonyms

        protected void AddSynonym(ushort scriptNum, string w1, string w2)
        {
            var res = _translate.GetResource<ResScript>(scriptNum);
            var scr = res.GetScript() as Script;

            var sections = scr.Get<SynonymSecion>();
            SynonymSecion section = sections.FirstOrDefault();
            if (section == null)
                section = scr.CreateSection(SectionType.Synonym) as SynonymSecion;

            var w1Ids = _translate.GetWordId(w1);
            //if (w1Ids == null || w1Ids.Length > 1) throw new Exception();
            var w2Ids = _translate.GetWordId(w2);
            //if (w2Ids == null || w2Ids.Length > 1) throw new Exception();

            var id1 = w1Ids[0];
            var id2 = w2Ids[0];

            if (section.Synonyms.Exists(s => (s.WordA == id1 && s.WordB == id2) || (s.WordA == id2 && s.WordB == id1)))
                return;

            section.Synonyms.Add(new Synonym
            {
                WordA = w1Ids[0],
                WordB = w2Ids[0]
            });

            Changed(res);
        }

        protected void RemoveSynonym(ushort scriptNum, string w1, string w2)
        {
            var w1Id = _translate.GetWordId(w1)[0];
            var w2Id = _translate.GetWordId(w2)[0];

            var res = _translate.GetResource<ResScript>(scriptNum);
            var scr = res.GetScript() as Script;

            SynonymSecion section = scr.SynonymSecion;
            for (int i = 0; i < section.Synonyms.Count; i++)
            {
                var syn = section.Synonyms[i];
                if ((syn.WordA == w1Id && syn.WordB == w2Id) || (syn.WordA == w2Id && syn.WordB == w1Id))
                {
                    section.Synonyms.RemoveAt(i);
                    Changed(res);
                    i--;
                }
            }
        }

        protected void RemoveSynonyms(ushort scriptNum, string word)
        {
            var wid = _translate.GetWordId(word)[0];
            var res = _translate.GetResource<ResScript>(scriptNum);
            var scr = res.GetScript() as Script;

            SynonymSecion section = scr.SynonymSecion;
            for (int i = 0; i < section.Synonyms.Count; i++)
            {
                var syn = section.Synonyms[i];
                if (syn.WordA == wid || syn.WordB == wid)
                {
                    section.Synonyms.RemoveAt(i);
                    Changed(res);
                    i--;
                }
            }
        }

        protected void RemoveSynDubl()
        {
            foreach (var res in _translate.Scripts)
            {
                var scr = res.GetScript() as Script;
                foreach (var synSec in scr.Get<SynonymSecion>())
                {
                    for (int i = 0; i < synSec.Synonyms.Count; i++)
                    {
                        var syn = synSec.Synonyms[i];
                        if (syn.WordA == syn.WordB)
                        {
                            synSec.Synonyms.RemoveAt(i);
                            i--;
                            Changed(res);
                        }
                    }
                }
            }
        }

        #endregion

        #region Saids

        protected void PrintSaids(ushort scriptNum)
        {
            var res = _translate.GetResource<ResScript>(scriptNum);
            var scr = res.GetScript() as Script;
            var saidSection = scr.SaidSection;
            for (int i = 0; i < saidSection.Saids.Count; i++)
            {
                var said = saidSection.Saids[i];
                Console.WriteLine($"{i} = {said}     {said.Hex}");
            }
        }

        protected void PatchSaid(ushort scriptNum, int ind, string str)
        {
            var res = _translate.GetResource<ResScript>(scriptNum);
            var scr = res.GetScript() as Script;
            if (scr.SaidSection.Saids[ind].Set(str))
                Changed(res);
        }

        #endregion

        #region Script

        protected void SetPushi(BaseScript scr, ushort addr, int val)
        {
            var op = scr.GetOperator(addr);
            if (op.Name != "pushi") throw new Exception();

            if (op.Arguments[0] is ShortArg s)
            {
                if (s.Value == val) return;
                s.Value = (short)val;
            }
            else if (op.Arguments[0] is ByteArg b)
            {
                if (b.Value == val) return;
                b.Value = (byte)val;
            }
            else throw new Exception();

            Changed(scr.Resource);
        }

        protected void SetLdi(BaseScript scr, ushort addr, int val)
        {
            var op = scr.GetOperator(addr);
            if (op.Name != "ldi") throw new Exception();

            if (op.Arguments[0] is ShortArg s)
            {
                if (s.Value == val) return;
                s.Value = (short)val;
            }
            else if (op.Arguments[0] is ByteArg b)
            {
                if (b.Value == val) return;
                b.Value = (byte)val;
            }
            else throw new Exception();

            Changed(scr.Resource);
        }

        protected void SetProperty(BaseScript scr, IScriptInstance inst, string name, ushort value)
        {
            if (inst.GetProperty(name) != value)
            {
                inst.SetProperty(name, value);
                Changed(scr.Resource);
            }
        }

        #endregion
    }
}
