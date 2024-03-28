namespace SCI_Translator
{
    partial class FormMain
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            sc = new System.Windows.Forms.SplitContainer();
            tv = new System.Windows.Forms.TreeView();
            ilTree = new System.Windows.Forms.ImageList(components);
            cmsResource = new System.Windows.Forms.ContextMenuStrip(components);
            tcmiExportToFile = new System.Windows.Forms.ToolStripMenuItem();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsbSave = new System.Windows.Forms.ToolStripButton();
            tsbTranslated = new System.Windows.Forms.ToolStripButton();
            tsbFind = new System.Windows.Forms.ToolStripButton();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            tsslResourceInfo = new System.Windows.Forms.ToolStripStatusLabel();
            tsbTextBoxDraw = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)sc).BeginInit();
            sc.Panel1.SuspendLayout();
            sc.SuspendLayout();
            cmsResource.SuspendLayout();
            toolStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // sc
            // 
            sc.Dock = System.Windows.Forms.DockStyle.Fill;
            sc.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            sc.Location = new System.Drawing.Point(0, 27);
            sc.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            sc.Name = "sc";
            // 
            // sc.Panel1
            // 
            sc.Panel1.Controls.Add(tv);
            sc.Size = new System.Drawing.Size(1053, 939);
            sc.SplitterDistance = 227;
            sc.SplitterWidth = 5;
            sc.TabIndex = 0;
            // 
            // tv
            // 
            tv.Dock = System.Windows.Forms.DockStyle.Fill;
            tv.HideSelection = false;
            tv.ImageIndex = 0;
            tv.ImageList = ilTree;
            tv.Location = new System.Drawing.Point(0, 0);
            tv.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            tv.Name = "tv";
            tv.SelectedImageIndex = 0;
            tv.Size = new System.Drawing.Size(227, 939);
            tv.TabIndex = 0;
            tv.AfterSelect += tv_AfterSelect;
            tv.NodeMouseClick += tv_NodeMouseClick;
            // 
            // ilTree
            // 
            ilTree.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            ilTree.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("ilTree.ImageStream");
            ilTree.TransparentColor = System.Drawing.Color.Transparent;
            ilTree.Images.SetKeyName(0, "folder");
            ilTree.Images.SetKeyName(1, "file");
            ilTree.Images.SetKeyName(2, "book");
            ilTree.Images.SetKeyName(3, "image");
            ilTree.Images.SetKeyName(4, "character");
            ilTree.Images.SetKeyName(5, "script");
            ilTree.Images.SetKeyName(6, "music");
            ilTree.Images.SetKeyName(7, "sound");
            ilTree.Images.SetKeyName(8, "font");
            ilTree.Images.SetKeyName(9, "cursor");
            ilTree.Images.SetKeyName(10, "palette");
            // 
            // cmsResource
            // 
            cmsResource.ImageScalingSize = new System.Drawing.Size(20, 20);
            cmsResource.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tcmiExportToFile });
            cmsResource.Name = "cmsTree";
            cmsResource.Size = new System.Drawing.Size(174, 28);
            // 
            // tcmiExportToFile
            // 
            tcmiExportToFile.Name = "tcmiExportToFile";
            tcmiExportToFile.Size = new System.Drawing.Size(173, 24);
            tcmiExportToFile.Text = "Export to file...";
            tcmiExportToFile.Click += tcmiExportToFile_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbSave, tsbTranslated, tsbFind, tsbTextBoxDraw });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1053, 27);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsbSave
            // 
            tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbSave.Image = Properties.Resources.disk;
            tsbSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbSave.Name = "tsbSave";
            tsbSave.Size = new System.Drawing.Size(29, 24);
            tsbSave.Text = "Save";
            tsbSave.Click += tsbSave_Click;
            // 
            // tsbTranslated
            // 
            tsbTranslated.Checked = true;
            tsbTranslated.CheckOnClick = true;
            tsbTranslated.CheckState = System.Windows.Forms.CheckState.Checked;
            tsbTranslated.Image = Properties.Resources.font;
            tsbTranslated.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbTranslated.Name = "tsbTranslated";
            tsbTranslated.Size = new System.Drawing.Size(101, 24);
            tsbTranslated.Text = "Translated";
            tsbTranslated.CheckedChanged += tsbTranslated_CheckedChanged;
            // 
            // tsbFind
            // 
            tsbFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbFind.Image = Properties.Resources.find;
            tsbFind.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbFind.Name = "tsbFind";
            tsbFind.Size = new System.Drawing.Size(29, 24);
            tsbFind.Text = "Find";
            tsbFind.Click += tsbFind_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsslResourceInfo });
            statusStrip1.Location = new System.Drawing.Point(0, 966);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            statusStrip1.Size = new System.Drawing.Size(1053, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // tsslResourceInfo
            // 
            tsslResourceInfo.Name = "tsslResourceInfo";
            tsslResourceInfo.Size = new System.Drawing.Size(0, 16);
            // 
            // tsbTextBoxDraw
            // 
            tsbTextBoxDraw.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbTextBoxDraw.Image = Properties.Resources.type;
            tsbTextBoxDraw.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbTextBoxDraw.Name = "tsbTextBoxDraw";
            tsbTextBoxDraw.Size = new System.Drawing.Size(29, 24);
            tsbTextBoxDraw.Text = "toolStripButton1";
            tsbTextBoxDraw.Click += tsbTextBoxDraw_Click;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1053, 988);
            Controls.Add(sc);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            Name = "FormMain";
            Text = "SCI Translator";
            sc.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)sc).EndInit();
            sc.ResumeLayout(false);
            cmsResource.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer sc;
        private System.Windows.Forms.TreeView tv;
        private System.Windows.Forms.ImageList ilTree;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsslResourceInfo;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.ToolStripButton tsbTranslated;
        private System.Windows.Forms.ToolStripButton tsbFind;
        private System.Windows.Forms.ContextMenuStrip cmsResource;
        private System.Windows.Forms.ToolStripMenuItem tcmiExportToFile;
        private System.Windows.Forms.ToolStripButton tsbTextBoxDraw;
    }
}

