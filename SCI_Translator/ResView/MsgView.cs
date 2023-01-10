using System.Windows.Forms;
using SCI_Lib.Resources;

namespace SCI_Translator.ResView
{
    class MsgView : ResViewer
    {
        private DataGridViewTextBoxColumn colTexID;
        private DataGridViewTextBoxColumn colNoun;
        private DataGridViewTextBoxColumn colVerb;
        private DataGridViewTextBoxColumn colCond;
        private DataGridViewTextBoxColumn colSeq;
        private DataGridViewTextBoxColumn colTalker;
        private DataGridViewTextBoxColumn colText;
        private System.Windows.Forms.DataGridView dgvText;

        public MsgView()
        {
            InitializeComponent();
        }
        public override bool DiffTranslate => false;

        protected override void Reload()
        {
            dgvText.Rows.Clear();

            var messages = ((ResMessage)Current).GetMessages();
            for (int i = 0; i < messages.Count; i++)
            {
                var m = messages[i];
                dgvText.Rows.Add(i, m.Noun, m.Verb, m.Cond, m.Seq, m.Talker, m.Text);
            }
        }

        public override void FocusRow(int value)
        {
            dgvText.CurrentCell = dgvText.Rows[value].Cells[0];
        }

        protected override void SaveContent()
        {
            dgvText.CommitEdit(DataGridViewDataErrorContexts.Commit);

            var tr = ((ResMessage)Current).GetMessages();
            for (int i = 0; i < tr.Count; i++)
            {
                tr[i].Text = (string)dgvText[6, i].Value;
            }
            Current.SavePatch();
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvText = new System.Windows.Forms.DataGridView();
            this.colTexID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNoun = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVerb = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCond = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTalker = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvText)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvText
            // 
            this.dgvText.AllowUserToAddRows = false;
            this.dgvText.AllowUserToDeleteRows = false;
            this.dgvText.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvText.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvText.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTexID,
            this.colNoun,
            this.colVerb,
            this.colCond,
            this.colSeq,
            this.colTalker,
            this.colText});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvText.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvText.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvText.Location = new System.Drawing.Point(0, 0);
            this.dgvText.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dgvText.MultiSelect = false;
            this.dgvText.Name = "dgvText";
            this.dgvText.RowHeadersVisible = false;
            this.dgvText.RowHeadersWidth = 51;
            this.dgvText.Size = new System.Drawing.Size(1357, 1205);
            this.dgvText.TabIndex = 1;
            // 
            // colTexID
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.colTexID.DefaultCellStyle = dataGridViewCellStyle1;
            this.colTexID.HeaderText = "TextID";
            this.colTexID.MinimumWidth = 6;
            this.colTexID.Name = "colTexID";
            this.colTexID.ReadOnly = true;
            this.colTexID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colTexID.Width = 50;
            // 
            // colNoun
            // 
            this.colNoun.HeaderText = "Noun";
            this.colNoun.MinimumWidth = 6;
            this.colNoun.Name = "colNoun";
            this.colNoun.ReadOnly = true;
            this.colNoun.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colNoun.Width = 50;
            // 
            // colVerb
            // 
            this.colVerb.HeaderText = "Verb";
            this.colVerb.MinimumWidth = 6;
            this.colVerb.Name = "colVerb";
            this.colVerb.ReadOnly = true;
            this.colVerb.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colVerb.Width = 50;
            // 
            // colCond
            // 
            this.colCond.HeaderText = "Cond";
            this.colCond.MinimumWidth = 6;
            this.colCond.Name = "colCond";
            this.colCond.ReadOnly = true;
            this.colCond.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colCond.Width = 50;
            // 
            // colSeq
            // 
            this.colSeq.HeaderText = "Seq";
            this.colSeq.MinimumWidth = 6;
            this.colSeq.Name = "colSeq";
            this.colSeq.ReadOnly = true;
            this.colSeq.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colSeq.Width = 50;
            // 
            // colTalker
            // 
            this.colTalker.HeaderText = "Talker";
            this.colTalker.MinimumWidth = 6;
            this.colTalker.Name = "colTalker";
            this.colTalker.ReadOnly = true;
            this.colTalker.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colTalker.Width = 50;
            // 
            // colText
            // 
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.colText.DefaultCellStyle = dataGridViewCellStyle2;
            this.colText.HeaderText = "Text";
            this.colText.MinimumWidth = 6;
            this.colText.Name = "colText";
            this.colText.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colText.Width = 600;
            // 
            // MsgView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.Controls.Add(this.dgvText);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MsgView";
            this.Size = new System.Drawing.Size(1357, 1205);
            ((System.ComponentModel.ISupportInitialize)(this.dgvText)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
