namespace SCI_Translator
{
    partial class FormFind
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.tbPattern = new System.Windows.Forms.TextBox();
            this.btFind = new System.Windows.Forms.Button();
            this.lbResults = new System.Windows.Forms.ListBox();
            this.chbExact = new System.Windows.Forms.CheckBox();
            this.chbMatchCase = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Text:";
            // 
            // tbPattern
            // 
            this.tbPattern.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPattern.Location = new System.Drawing.Point(16, 31);
            this.tbPattern.Margin = new System.Windows.Forms.Padding(4);
            this.tbPattern.Name = "tbPattern";
            this.tbPattern.Size = new System.Drawing.Size(532, 22);
            this.tbPattern.TabIndex = 1;
            // 
            // btFind
            // 
            this.btFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btFind.Location = new System.Drawing.Point(556, 28);
            this.btFind.Margin = new System.Windows.Forms.Padding(4);
            this.btFind.Name = "btFind";
            this.btFind.Size = new System.Drawing.Size(100, 28);
            this.btFind.TabIndex = 2;
            this.btFind.Text = "Find";
            this.btFind.UseVisualStyleBackColor = true;
            this.btFind.Click += new System.EventHandler(this.btFind_Click);
            // 
            // lbResults
            // 
            this.lbResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbResults.FormattingEnabled = true;
            this.lbResults.ItemHeight = 16;
            this.lbResults.Location = new System.Drawing.Point(16, 96);
            this.lbResults.Margin = new System.Windows.Forms.Padding(4);
            this.lbResults.Name = "lbResults";
            this.lbResults.Size = new System.Drawing.Size(640, 516);
            this.lbResults.TabIndex = 3;
            this.lbResults.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbResults_MouseDoubleClick);
            // 
            // chbExact
            // 
            this.chbExact.AutoSize = true;
            this.chbExact.Location = new System.Drawing.Point(16, 60);
            this.chbExact.Name = "chbExact";
            this.chbExact.Size = new System.Drawing.Size(64, 21);
            this.chbExact.TabIndex = 4;
            this.chbExact.Text = "Exact";
            this.chbExact.UseVisualStyleBackColor = true;
            // 
            // chbMatchCase
            // 
            this.chbMatchCase.AutoSize = true;
            this.chbMatchCase.Location = new System.Drawing.Point(120, 60);
            this.chbMatchCase.Name = "chbMatchCase";
            this.chbMatchCase.Size = new System.Drawing.Size(102, 21);
            this.chbMatchCase.TabIndex = 5;
            this.chbMatchCase.Text = "Match case";
            this.chbMatchCase.UseVisualStyleBackColor = true;
            // 
            // FormFind
            // 
            this.AcceptButton = this.btFind;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(673, 632);
            this.Controls.Add(this.chbMatchCase);
            this.Controls.Add(this.chbExact);
            this.Controls.Add(this.lbResults);
            this.Controls.Add(this.btFind);
            this.Controls.Add(this.tbPattern);
            this.Controls.Add(this.label1);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormFind";
            this.ShowIcon = false;
            this.Text = "Search";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormFind_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbPattern;
        private System.Windows.Forms.Button btFind;
        private System.Windows.Forms.ListBox lbResults;
        private System.Windows.Forms.CheckBox chbExact;
        private System.Windows.Forms.CheckBox chbMatchCase;
    }
}