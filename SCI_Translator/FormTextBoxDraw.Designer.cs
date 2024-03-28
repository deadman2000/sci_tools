namespace SCI_Translator
{
    partial class FormTextBoxDraw
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
            label1 = new System.Windows.Forms.Label();
            cbFont = new System.Windows.Forms.ComboBox();
            label2 = new System.Windows.Forms.Label();
            nudWidth = new System.Windows.Forms.NumericUpDown();
            label3 = new System.Windows.Forms.Label();
            nudLineHeight = new System.Windows.Forms.NumericUpDown();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            pbRender = new System.Windows.Forms.PictureBox();
            tbText = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)nudWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudLineHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbRender).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 18);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(41, 20);
            label1.TabIndex = 0;
            label1.Text = "Font:";
            // 
            // cbFont
            // 
            cbFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbFont.FormattingEnabled = true;
            cbFont.Location = new System.Drawing.Point(59, 12);
            cbFont.Name = "cbFont";
            cbFont.Size = new System.Drawing.Size(149, 28);
            cbFont.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(214, 18);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(52, 20);
            label2.TabIndex = 2;
            label2.Text = "Width:";
            // 
            // nudWidth
            // 
            nudWidth.Location = new System.Drawing.Point(272, 13);
            nudWidth.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudWidth.Name = "nudWidth";
            nudWidth.Size = new System.Drawing.Size(110, 27);
            nudWidth.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(388, 18);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(85, 20);
            label3.TabIndex = 2;
            label3.Text = "Line height:";
            // 
            // nudLineHeight
            // 
            nudLineHeight.Location = new System.Drawing.Point(479, 13);
            nudLineHeight.Name = "nudLineHeight";
            nudLineHeight.Size = new System.Drawing.Size(110, 27);
            nudLineHeight.TabIndex = 3;
            nudLineHeight.Value = new decimal(new int[] { 9, 0, 0, 0 });
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            splitContainer1.Location = new System.Drawing.Point(1, 46);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(pbRender);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tbText);
            splitContainer1.Size = new System.Drawing.Size(917, 523);
            splitContainer1.SplitterDistance = 532;
            splitContainer1.TabIndex = 4;
            // 
            // pbRender
            // 
            pbRender.BackColor = System.Drawing.Color.Magenta;
            pbRender.Dock = System.Windows.Forms.DockStyle.Fill;
            pbRender.Location = new System.Drawing.Point(0, 0);
            pbRender.Name = "pbRender";
            pbRender.Size = new System.Drawing.Size(532, 523);
            pbRender.TabIndex = 0;
            pbRender.TabStop = false;
            // 
            // tbText
            // 
            tbText.Dock = System.Windows.Forms.DockStyle.Fill;
            tbText.Location = new System.Drawing.Point(0, 0);
            tbText.Multiline = true;
            tbText.Name = "tbText";
            tbText.Size = new System.Drawing.Size(381, 523);
            tbText.TabIndex = 0;
            tbText.Text = "Simple text...";
            // 
            // FormTextBoxDraw
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(917, 568);
            Controls.Add(splitContainer1);
            Controls.Add(nudLineHeight);
            Controls.Add(label3);
            Controls.Add(nudWidth);
            Controls.Add(label2);
            Controls.Add(cbFont);
            Controls.Add(label1);
            Name = "FormTextBoxDraw";
            Text = "FormTextBoxDraw";
            ((System.ComponentModel.ISupportInitialize)nudWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudLineHeight).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbRender).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbFont;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudWidth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudLineHeight;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pbRender;
        private System.Windows.Forms.TextBox tbText;
    }
}