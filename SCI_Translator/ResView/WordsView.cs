using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Resources.Vocab;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace SCI_Translator.ResView
{
    class WordsView : ResViewer
    {
        private Panel panel1;
        private TextBox tbSearch;
        private DataGridView dgvWords;

        private List<WordTranslate> _dataSet;
        private DataGridViewTextBoxColumn colClass;
        private DataGridViewTextBoxColumn colGroup;
        private DataGridViewTextBoxColumn colWordsSrc;
        private DataGridViewTextBoxColumn colTranslate;
        private DataGridViewTextBoxColumn colUsed;
        private ResVocab001 _trvoc001;

        public WordsView()
        {
            InitializeComponent();
        }

        public override bool DiffTranslate => false;

        public override bool IsAutoSave => true;

        protected override void Reload()
        {
            dgvWords.AutoGenerateColumns = false;
            dgvWords.DataSource = null;

            _dataSet = new List<WordTranslate>();

            var words = ((ResVocab000)_res).GetWords();
            foreach (var gr in words.GroupBy(w => w.Group).OrderBy(g => g.Key))
            {
                if (gr.Key > 0xffd) continue;

                foreach (var clGr in gr.GroupBy(g => g.Class).OrderBy(g => g.Key))
                {
                    var groupWords = string.Join(',', clGr.Select(w => w.Text));
                    _dataSet.Add(new WordTranslate
                    {
                        Cl = clGr.Key,
                        Group = gr.Key,
                        Src = groupWords
                    });
                }
            }

            if (_tres != null)
            {
                colTranslate.Visible = true;
                var voc = (ResVocab001)_tres.Package.GetResource(ResType.Vocabulary, 1);
                voc ??= (ResVocab001)_tres.Package.AddResource(ResType.Vocabulary, 1);
                LoadTranslate(voc);
            }
            else
            {
                colTranslate.Visible = false;
            }
            CheckUsage();

            dgvWords.DataSource = _dataSet;
        }

        private void LoadTranslate(ResVocab001 voc)
        {
            _trvoc001 = voc;
            var words = voc.GetWords();

            foreach (var gr in words.GroupBy(w => w.Id))
            {
                var groupWords = string.Join(',', gr.Select(w => w.Text));
                var first = gr.First();

                var wt = _dataSet.FirstOrDefault(w => w.Cl == first.Class && w.Group == first.Group);
                if (wt != null)
                {
                    wt.Translate = groupWords;
                }
                else
                {
                    _dataSet.Add(new WordTranslate
                    {
                        Cl = first.Class,
                        Group = first.Group,
                        Translate = groupWords
                    });
                }
            }
        }

        protected override void SaveContent()
        {
            if (_trvoc001 == null) return;

            dgvWords.CommitEdit(DataGridViewDataErrorContexts.Commit);
            List<Word> words = new();
            HashSet<string> hashs = new();

            foreach (var item in _dataSet)
            {
                if (string.IsNullOrWhiteSpace(item.Translate)) continue;

                foreach (var txt in item.Translate.Split(','))
                {
                    var word = new Word(txt.Trim().ToLower(), (ushort)item.Cl, item.Group);
                    var hash = word.Text + "_" + word.Id;
                    if (hashs.Contains(hash)) continue;
                    hashs.Add(hash);
                    words.Add(word);
                }
            }

            _trvoc001.SetWords(words);
            _trvoc001.SavePatch();
            _tres.Package.ResetWords();
            _tres.Package.CleanCache();
        }

        private void CheckUsage()
        {
            var package = _tres != null ? _tres.Package : _res.Package;
            var resources = package.Scripts
                .GroupBy(r => r.Number).Select(g => g.First());

            var scripts = resources.Select(r => r.GetScript() as Script)
                .Where(s => s != null)
                .ToList();

            var wordsUsage = scripts
                .SelectMany(s => s.Get<SaidSection>().SelectMany(ss => ss.Saids)
                        .SelectMany(s => s.Expression)
                        .Where(e => !e.IsOperator)
                        .Select(s => s.Data)
                        .Union(s.Get<SynonymSecion>().SelectMany(s => s.Synonyms).Select(s => s.WordA))
                        .Distinct()
                        .Select(w => new { S = s, W = w })
                )
                .GroupBy(i => i.W)
                .ToDictionary(g => g.Key, g => string.Join(',', g.Select(n => n.S.Resource.Number.ToString())));


            foreach (var wt in _dataSet)
            {
                if (wordsUsage.TryGetValue(wt.Group, out var str))
                    wt.UsedIn = str;
            }
        }

        private void InitializeComponent()
        {
            dgvWords = new DataGridView();
            panel1 = new Panel();
            tbSearch = new TextBox();
            colClass = new DataGridViewTextBoxColumn();
            colGroup = new DataGridViewTextBoxColumn();
            colWordsSrc = new DataGridViewTextBoxColumn();
            colTranslate = new DataGridViewTextBoxColumn();
            colUsed = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dgvWords).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // dgvWords
            // 
            dgvWords.AllowUserToAddRows = false;
            dgvWords.AllowUserToDeleteRows = false;
            dgvWords.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvWords.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvWords.Columns.AddRange(new DataGridViewColumn[] { colClass, colGroup, colWordsSrc, colTranslate, colUsed });
            dgvWords.Dock = DockStyle.Fill;
            dgvWords.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvWords.Location = new System.Drawing.Point(0, 42);
            dgvWords.Margin = new Padding(4);
            dgvWords.Name = "dgvWords";
            dgvWords.RowHeadersVisible = false;
            dgvWords.RowHeadersWidth = 51;
            dgvWords.RowTemplate.Height = 29;
            dgvWords.Size = new System.Drawing.Size(1868, 1126);
            dgvWords.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(tbSearch);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1868, 42);
            panel1.TabIndex = 1;
            // 
            // tbSearch
            // 
            tbSearch.Location = new System.Drawing.Point(8, 4);
            tbSearch.Name = "tbSearch";
            tbSearch.Size = new System.Drawing.Size(406, 27);
            tbSearch.TabIndex = 0;
            tbSearch.TextChanged += tbSearch_TextChanged;
            // 
            // colClass
            // 
            colClass.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            colClass.DataPropertyName = "ClStr";
            colClass.HeaderText = "Class";
            colClass.MinimumWidth = 6;
            colClass.Name = "colClass";
            colClass.ReadOnly = true;
            colClass.Width = 71;
            // 
            // colGroup
            // 
            colGroup.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            colGroup.DataPropertyName = "GroupHex";
            colGroup.HeaderText = "Group";
            colGroup.MinimumWidth = 6;
            colGroup.Name = "colGroup";
            colGroup.ReadOnly = true;
            colGroup.Width = 79;
            // 
            // colWordsSrc
            // 
            colWordsSrc.DataPropertyName = "Src";
            colWordsSrc.HeaderText = "Words";
            colWordsSrc.MinimumWidth = 6;
            colWordsSrc.Name = "colWordsSrc";
            colWordsSrc.ReadOnly = true;
            colWordsSrc.Width = 125;
            // 
            // colTranslate
            // 
            colTranslate.DataPropertyName = "Translate";
            colTranslate.HeaderText = "Translate";
            colTranslate.MinimumWidth = 6;
            colTranslate.Name = "colTranslate";
            colTranslate.Width = 125;
            // 
            // colUsed
            // 
            colUsed.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            colUsed.DataPropertyName = "UsedIn";
            colUsed.HeaderText = "Used";
            colUsed.MinimumWidth = 6;
            colUsed.Name = "colUsed";
            colUsed.ReadOnly = true;
            colUsed.Width = 71;
            // 
            // WordsView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            Controls.Add(dgvWords);
            Controls.Add(panel1);
            Name = "WordsView";
            Size = new System.Drawing.Size(1868, 1168);
            ((System.ComponentModel.ISupportInitialize)dgvWords).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            dgvWords.SuspendLayout();
            SuspendLayout();

            var txt = tbSearch.Text;
            if (!string.IsNullOrWhiteSpace(txt))
            {
                var filtered = _dataSet.Where(w => (w.Src != null && w.Src.Contains(txt)) || (w.Translate != null && w.Translate.Contains(txt)));
                dgvWords.DataSource = filtered.ToList();
            }
            else
            {
                dgvWords.DataSource = _dataSet;
            }

            dgvWords.ResumeLayout();
            ResumeLayout();
        }

        class WordTranslate
        {
            public WordClass Cl { get; set; }
            public string ClStr => $"{Cl} [{(int)Cl:X03}]";
            public ushort Group { get; set; }
            public string GroupHex => $"{Group:X03}";
            public string Src { get; set; }
            public string Translate { get; set; }
            public string UsedIn { get; set; }
        }
    }
}
