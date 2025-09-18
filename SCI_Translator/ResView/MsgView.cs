using System.Windows.Forms;
using SCI_Lib.Resources;

namespace SCI_Translator.ResView
{
    class MsgView : ResViewer
    {
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
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            dgvText = new DataGridView();
            colTexID = new DataGridViewTextBoxColumn();
            colNoun = new DataGridViewTextBoxColumn();
            colVerb = new DataGridViewTextBoxColumn();
            colCond = new DataGridViewTextBoxColumn();
            colSeq = new DataGridViewTextBoxColumn();
            colTalker = new DataGridViewTextBoxColumn();
            colText = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dgvText).BeginInit();
            SuspendLayout();
            // 
            // dgvText
            // 
            dgvText.AllowUserToAddRows = false;
            dgvText.AllowUserToDeleteRows = false;
            dgvText.AllowUserToOrderColumns = true;
            dgvText.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvText.BorderStyle = BorderStyle.Fixed3D;
            dgvText.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvText.Columns.AddRange(new DataGridViewColumn[] { colTexID, colNoun, colVerb, colCond, colSeq, colTalker, colText });
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvText.DefaultCellStyle = dataGridViewCellStyle3;
            dgvText.Dock = DockStyle.Fill;
            dgvText.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvText.Location = new System.Drawing.Point(0, 0);
            dgvText.Margin = new Padding(4, 5, 4, 5);
            dgvText.MultiSelect = false;
            dgvText.Name = "dgvText";
            dgvText.RowHeadersVisible = false;
            dgvText.RowHeadersWidth = 51;
            dgvText.Size = new System.Drawing.Size(1829, 1135);
            dgvText.TabIndex = 1;
            // 
            // colTexID
            // 
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTexID.DefaultCellStyle = dataGridViewCellStyle1;
            colTexID.HeaderText = "TextID";
            colTexID.MinimumWidth = 6;
            colTexID.Name = "colTexID";
            colTexID.ReadOnly = true;
            colTexID.Width = 50;
            // 
            // colNoun
            // 
            colNoun.HeaderText = "Noun";
            colNoun.MinimumWidth = 6;
            colNoun.Name = "colNoun";
            colNoun.ReadOnly = true;
            colNoun.Width = 50;
            // 
            // colVerb
            // 
            colVerb.HeaderText = "Verb";
            colVerb.MinimumWidth = 6;
            colVerb.Name = "colVerb";
            colVerb.ReadOnly = true;
            colVerb.Width = 50;
            // 
            // colCond
            // 
            colCond.HeaderText = "Cond";
            colCond.MinimumWidth = 6;
            colCond.Name = "colCond";
            colCond.ReadOnly = true;
            colCond.Width = 50;
            // 
            // colSeq
            // 
            colSeq.HeaderText = "Seq";
            colSeq.MinimumWidth = 6;
            colSeq.Name = "colSeq";
            colSeq.ReadOnly = true;
            colSeq.Width = 50;
            // 
            // colTalker
            // 
            colTalker.HeaderText = "Talker";
            colTalker.MinimumWidth = 6;
            colTalker.Name = "colTalker";
            colTalker.ReadOnly = true;
            colTalker.Width = 50;
            // 
            // colText
            // 
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            colText.DefaultCellStyle = dataGridViewCellStyle2;
            colText.HeaderText = "Text";
            colText.MinimumWidth = 6;
            colText.Name = "colText";
            colText.SortMode = DataGridViewColumnSortMode.NotSortable;
            colText.Width = 600;
            // 
            // MsgView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            Controls.Add(dgvText);
            Margin = new Padding(4, 5, 4, 5);
            Name = "MsgView";
            Size = new System.Drawing.Size(1829, 1135);
            ((System.ComponentModel.ISupportInitialize)dgvText).EndInit();
            ResumeLayout(false);

        }
        private DataGridViewTextBoxColumn colTexID;
        private DataGridViewTextBoxColumn colNoun;
        private DataGridViewTextBoxColumn colVerb;
        private DataGridViewTextBoxColumn colCond;
        private DataGridViewTextBoxColumn colSeq;
        private DataGridViewTextBoxColumn colTalker;
        private DataGridViewTextBoxColumn colText;
    }
}
