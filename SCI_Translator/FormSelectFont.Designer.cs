
namespace SCI_Translator
{
    partial class FormSelectFont
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
            this.components = new System.ComponentModel.Container();
            this.icbFont = new SCI_Translator.Components.ImageComboBox();
            this.ilFonts = new System.Windows.Forms.ImageList(this.components);
            this.nudStart = new System.Windows.Forms.NumericUpDown();
            this.nudEnd = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btCancel = new System.Windows.Forms.Button();
            this.btOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEnd)).BeginInit();
            this.SuspendLayout();
            // 
            // icbFont
            // 
            this.icbFont.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.icbFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.icbFont.FormattingEnabled = true;
            this.icbFont.Images = this.ilFonts;
            this.icbFont.ItemHeight = 64;
            this.icbFont.Location = new System.Drawing.Point(12, 12);
            this.icbFont.Name = "icbFont";
            this.icbFont.Size = new System.Drawing.Size(320, 70);
            this.icbFont.TabIndex = 0;
            // 
            // ilFonts
            // 
            this.ilFonts.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ilFonts.ImageSize = new System.Drawing.Size(64, 64);
            this.ilFonts.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // nudStart
            // 
            this.nudStart.Location = new System.Drawing.Point(74, 88);
            this.nudStart.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudStart.Name = "nudStart";
            this.nudStart.Size = new System.Drawing.Size(150, 26);
            this.nudStart.TabIndex = 1;
            this.nudStart.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // nudEnd
            // 
            this.nudEnd.Location = new System.Drawing.Point(74, 120);
            this.nudEnd.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudEnd.Name = "nudEnd";
            this.nudEnd.Size = new System.Drawing.Size(150, 26);
            this.nudEnd.TabIndex = 2;
            this.nudEnd.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 90);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "From:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 122);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "To:";
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(238, 161);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(94, 29);
            this.btCancel.TabIndex = 4;
            this.btCancel.Text = "Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(138, 161);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(94, 29);
            this.btOK.TabIndex = 5;
            this.btOK.Text = "OK";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // FormSelectFont
            // 
            this.AcceptButton = this.btOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(344, 209);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nudEnd);
            this.Controls.Add(this.nudStart);
            this.Controls.Add(this.icbFont);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSelectFont";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select font to outline";
            ((System.ComponentModel.ISupportInitialize)(this.nudStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEnd)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Components.ImageComboBox icbFont;
        private System.Windows.Forms.ImageList ilFonts;
        private System.Windows.Forms.NumericUpDown nudStart;
        private System.Windows.Forms.NumericUpDown nudEnd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOK;
    }
}