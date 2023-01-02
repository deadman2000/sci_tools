using SCI_Lib.Resources;
using System.Windows.Forms;

namespace SCI_Translator.ResView
{
    class TextViewer : ResViewer
    {
        private DataGridViewTextBoxColumn colTexID;
        private DataGridViewTextBoxColumn colTexEn;
        private DataGridViewTextBoxColumn colTextTr;
        private System.Windows.Forms.DataGridView dgvText;

        public TextViewer()
        {
            InitializeComponent();
        }

        public override bool DiffTranslate => false;

        protected override void Reload()
        {
            dgvText.Rows.Clear();

            ResText txt = (ResText)_res;
            var en = txt.GetStrings();

            if (_tres != null)
            {
                var tr = _tres.GetStrings();
                for (int i = 0; i < en.Length; i++)
                    dgvText.Rows.Add(i, en[i], tr[i]);
            }
            else
            {
                for (int i = 0; i < en.Length; i++)
                    dgvText.Rows.Add(i, en[i], "");
            }
        }

        public override void FocusRow(int value)
        {
            dgvText.CurrentCell = dgvText[0, value];
        }

        protected override void SaveContent()
        {
            dgvText.CommitEdit(DataGridViewDataErrorContexts.Commit);

            string[] lines = new string[dgvText.Rows.Count];
            for (int r = 0; r < dgvText.Rows.Count; r++)
            {
                lines[r] = (string)dgvText[2, r].Value;
            }

            ((ResText)_tres).SetStrings(lines);
            _tres.SavePatch();
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvText = new System.Windows.Forms.DataGridView();
            this.colTexID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTexEn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTextTr = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.colTexEn,
            this.colTextTr});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvText.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvText.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvText.Location = new System.Drawing.Point(0, 0);
            this.dgvText.Margin = new System.Windows.Forms.Padding(4);
            this.dgvText.MultiSelect = false;
            this.dgvText.Name = "dgvText";
            this.dgvText.RowHeadersVisible = false;
            this.dgvText.RowHeadersWidth = 51;
            this.dgvText.Size = new System.Drawing.Size(1384, 1206);
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
            // colTexEn
            // 
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.colTexEn.DefaultCellStyle = dataGridViewCellStyle2;
            this.colTexEn.HeaderText = "Source";
            this.colTexEn.MinimumWidth = 6;
            this.colTexEn.Name = "colTexEn";
            this.colTexEn.ReadOnly = true;
            this.colTexEn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colTexEn.Width = 300;
            // 
            // colTextTr
            // 
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.colTextTr.DefaultCellStyle = dataGridViewCellStyle3;
            this.colTextTr.HeaderText = "Translate";
            this.colTextTr.MinimumWidth = 6;
            this.colTextTr.Name = "colTextTr";
            this.colTextTr.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colTextTr.Width = 300;
            // 
            // TextViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.Controls.Add(this.dgvText);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "TextViewer";
            this.Size = new System.Drawing.Size(1384, 1206);
            ((System.ComponentModel.ISupportInitialize)(this.dgvText)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
