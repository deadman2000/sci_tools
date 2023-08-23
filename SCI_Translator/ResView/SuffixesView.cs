using SCI_Lib.Resources;
using SCI_Lib.Resources.Vocab;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SCI_Translator.ResView
{
    class SuffixesView : ResViewer
    {
        private System.Windows.Forms.DataGridView dgvSuffixes;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPattern;
        private System.Windows.Forms.DataGridViewTextBoxColumn colInputClass;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOutput;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSuffixClass;
        private ToolStrip toolStrip1;
        private ToolStripButton tsbAddSuffix;
        private ToolStripButton tsbRemoveSuffix;
        private List<Suffix> _suffixes;

        public SuffixesView()
        {
            InitializeComponent();
        }

        protected override void Reload()
        {
            dgvSuffixes.AutoGenerateColumns = false;

            _suffixes = ((ResVocab901)Current).GetSuffixes().ToList();
            dgvSuffixes.DataSource = _suffixes;
            dgvSuffixes.AutoResizeColumns();
        }

        protected override void SaveContent()
        {
            dgvSuffixes.CommitEdit(DataGridViewDataErrorContexts.Commit);

            var voc = (ResVocab901)Current;
            voc.SetSuffixes(_suffixes.ToArray());
            Current.SavePatch();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SuffixesView));
            this.dgvSuffixes = new System.Windows.Forms.DataGridView();
            this.colPattern = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colInputClass = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOutput = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSuffixClass = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbAddSuffix = new System.Windows.Forms.ToolStripButton();
            this.tsbRemoveSuffix = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSuffixes)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvSuffixes
            // 
            this.dgvSuffixes.AllowUserToAddRows = false;
            this.dgvSuffixes.AllowUserToDeleteRows = false;
            this.dgvSuffixes.AllowUserToOrderColumns = true;
            this.dgvSuffixes.AllowUserToResizeRows = false;
            this.dgvSuffixes.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvSuffixes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvSuffixes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSuffixes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colPattern,
            this.colInputClass,
            this.colOutput,
            this.colSuffixClass});
            this.dgvSuffixes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSuffixes.Location = new System.Drawing.Point(0, 27);
            this.dgvSuffixes.MultiSelect = false;
            this.dgvSuffixes.Name = "dgvSuffixes";
            this.dgvSuffixes.RowHeadersVisible = false;
            this.dgvSuffixes.RowHeadersWidth = 51;
            this.dgvSuffixes.RowTemplate.Height = 29;
            this.dgvSuffixes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvSuffixes.ShowCellErrors = false;
            this.dgvSuffixes.ShowCellToolTips = false;
            this.dgvSuffixes.ShowEditingIcon = false;
            this.dgvSuffixes.ShowRowErrors = false;
            this.dgvSuffixes.Size = new System.Drawing.Size(1870, 791);
            this.dgvSuffixes.TabIndex = 0;
            // 
            // colPattern
            // 
            this.colPattern.DataPropertyName = "Pattern";
            this.colPattern.HeaderText = "Pattern";
            this.colPattern.MinimumWidth = 6;
            this.colPattern.Name = "colPattern";
            this.colPattern.Width = 125;
            // 
            // colInputClass
            // 
            this.colInputClass.DataPropertyName = "InputClassStr";
            this.colInputClass.HeaderText = "InputClass";
            this.colInputClass.MinimumWidth = 6;
            this.colInputClass.Name = "colInputClass";
            this.colInputClass.Width = 125;
            // 
            // colOutput
            // 
            this.colOutput.DataPropertyName = "Output";
            this.colOutput.HeaderText = "Output";
            this.colOutput.MinimumWidth = 6;
            this.colOutput.Name = "colOutput";
            this.colOutput.Width = 125;
            // 
            // colSuffixClass
            // 
            this.colSuffixClass.DataPropertyName = "SuffixClassStr";
            this.colSuffixClass.HeaderText = "SuffixClass";
            this.colSuffixClass.MinimumWidth = 6;
            this.colSuffixClass.Name = "colSuffixClass";
            this.colSuffixClass.Width = 125;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbAddSuffix,
            this.tsbRemoveSuffix});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1870, 27);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbAddSuffix
            // 
            this.tsbAddSuffix.Image = global::SCI_Translator.Properties.Resources.add;
            this.tsbAddSuffix.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAddSuffix.Name = "tsbAddSuffix";
            this.tsbAddSuffix.Size = new System.Drawing.Size(102, 24);
            this.tsbAddSuffix.Text = "Add Suffix";
            this.tsbAddSuffix.Click += new System.EventHandler(this.tsbAddSuffix_Click);
            // 
            // tsbRemoveSuffix
            // 
            this.tsbRemoveSuffix.Image = ((System.Drawing.Image)(resources.GetObject("tsbRemoveSuffix.Image")));
            this.tsbRemoveSuffix.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRemoveSuffix.Name = "tsbRemoveSuffix";
            this.tsbRemoveSuffix.Size = new System.Drawing.Size(128, 24);
            this.tsbRemoveSuffix.Text = "Remove Suffix";
            this.tsbRemoveSuffix.Click += new System.EventHandler(this.tsbRemoveSuffix_Click);
            // 
            // SuffixesView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.Controls.Add(this.dgvSuffixes);
            this.Controls.Add(this.toolStrip1);
            this.Name = "SuffixesView";
            this.Size = new System.Drawing.Size(1870, 818);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSuffixes)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void tsbAddSuffix_Click(object sender, System.EventArgs e)
        {
            _suffixes.Add(new Suffix("", 0, "", 0));
            dgvSuffixes.DataSource = null;
            dgvSuffixes.DataSource = _suffixes;
            var cell = dgvSuffixes.Rows[_suffixes.Count - 1].Cells[0];
            cell.Selected = true;
            dgvSuffixes.CurrentCell = cell;
            dgvSuffixes.BeginEdit(true);
        }

        private void tsbRemoveSuffix_Click(object sender, System.EventArgs e)
        {
            _suffixes.RemoveAt(dgvSuffixes.CurrentCell.RowIndex);
            dgvSuffixes.DataSource = null;
            dgvSuffixes.DataSource = _suffixes;
        }
    }
}
