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
                    var groupWords = string.Join(',', gr.Select(w => w.Text));
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
                voc ??= (ResVocab001)_tres.Package.CreateResource(ResType.Vocabulary, 1);
                LoadTranslate(voc);
            }
            else
            {
                colTranslate.Visible = false;
            }
            CheckUsage();

            dgvWords.DataSource = _dataSet;
            dgvWords.AutoResizeColumns();
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
        }

        private void CheckUsage()
        {
            var package = _tres != null ? _tres.Package : _res.Package;
            var resources = package.GetResources<ResScript>()
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
                .ToDictionary(g => g.Key, g => string.Join(',', g.Select(n => n.S.Resource.Number.ToString()).ToArray()));


            foreach (var wt in _dataSet)
            {
                if (wordsUsage.TryGetValue(wt.Group, out var str))
                    wt.UsedIn = str;
            }
        }

        private void InitializeComponent()
        {
            this.dgvWords = new System.Windows.Forms.DataGridView();
            this.colClass = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGroup = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWordsSrc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTranslate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUsed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbSearch = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWords)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvWords
            // 
            this.dgvWords.AllowUserToAddRows = false;
            this.dgvWords.AllowUserToDeleteRows = false;
            this.dgvWords.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvWords.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWords.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colClass,
            this.colGroup,
            this.colWordsSrc,
            this.colTranslate,
            this.colUsed});
            this.dgvWords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvWords.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvWords.Location = new System.Drawing.Point(0, 42);
            this.dgvWords.Margin = new System.Windows.Forms.Padding(4);
            this.dgvWords.Name = "dgvWords";
            this.dgvWords.RowHeadersVisible = false;
            this.dgvWords.RowHeadersWidth = 51;
            this.dgvWords.RowTemplate.Height = 29;
            this.dgvWords.Size = new System.Drawing.Size(1511, 1163);
            this.dgvWords.TabIndex = 0;
            // 
            // colClass
            // 
            this.colClass.DataPropertyName = "ClStr";
            this.colClass.HeaderText = "Class";
            this.colClass.MinimumWidth = 6;
            this.colClass.Name = "colClass";
            this.colClass.ReadOnly = true;
            this.colClass.Width = 125;
            // 
            // colGroup
            // 
            this.colGroup.DataPropertyName = "GroupHex";
            this.colGroup.HeaderText = "Group";
            this.colGroup.MinimumWidth = 6;
            this.colGroup.Name = "colGroup";
            this.colGroup.ReadOnly = true;
            this.colGroup.Width = 125;
            // 
            // colWordsSrc
            // 
            this.colWordsSrc.DataPropertyName = "Src";
            this.colWordsSrc.HeaderText = "Words";
            this.colWordsSrc.MinimumWidth = 6;
            this.colWordsSrc.Name = "colWordsSrc";
            this.colWordsSrc.ReadOnly = true;
            this.colWordsSrc.Width = 125;
            // 
            // colTranslate
            // 
            this.colTranslate.DataPropertyName = "Translate";
            this.colTranslate.HeaderText = "Translate";
            this.colTranslate.MinimumWidth = 6;
            this.colTranslate.Name = "colTranslate";
            this.colTranslate.Width = 125;
            // 
            // colUsed
            // 
            this.colUsed.DataPropertyName = "UsedIn";
            this.colUsed.HeaderText = "Used";
            this.colUsed.MinimumWidth = 6;
            this.colUsed.Name = "colUsed";
            this.colUsed.ReadOnly = true;
            this.colUsed.Width = 125;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tbSearch);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1511, 42);
            this.panel1.TabIndex = 1;
            // 
            // tbSearch
            // 
            this.tbSearch.Location = new System.Drawing.Point(8, 4);
            this.tbSearch.Name = "tbSearch";
            this.tbSearch.Size = new System.Drawing.Size(406, 27);
            this.tbSearch.TabIndex = 0;
            this.tbSearch.TextChanged += new System.EventHandler(this.tbSearch_TextChanged);
            // 
            // WordsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.Controls.Add(this.dgvWords);
            this.Controls.Add(this.panel1);
            this.Name = "WordsView";
            this.Size = new System.Drawing.Size(1511, 1205);
            ((System.ComponentModel.ISupportInitialize)(this.dgvWords)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

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
