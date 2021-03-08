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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.sc = new System.Windows.Forms.SplitContainer();
            this.tv = new System.Windows.Forms.TreeView();
            this.ilTree = new System.Windows.Forms.ImageList(this.components);
            this.cmsResource = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tcmiExportToFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.tsbTranslated = new System.Windows.Forms.ToolStripButton();
            this.tsbFind = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsslResourceInfo = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.sc)).BeginInit();
            this.sc.Panel1.SuspendLayout();
            this.sc.SuspendLayout();
            this.cmsResource.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // sc
            // 
            this.sc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sc.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.sc.Location = new System.Drawing.Point(0, 27);
            this.sc.Margin = new System.Windows.Forms.Padding(4);
            this.sc.Name = "sc";
            // 
            // sc.Panel1
            // 
            this.sc.Panel1.Controls.Add(this.tv);
            this.sc.Size = new System.Drawing.Size(1053, 741);
            this.sc.SplitterDistance = 227;
            this.sc.SplitterWidth = 5;
            this.sc.TabIndex = 0;
            // 
            // tv
            // 
            this.tv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tv.HideSelection = false;
            this.tv.ImageIndex = 0;
            this.tv.ImageList = this.ilTree;
            this.tv.Location = new System.Drawing.Point(0, 0);
            this.tv.Margin = new System.Windows.Forms.Padding(4);
            this.tv.Name = "tv";
            this.tv.SelectedImageIndex = 0;
            this.tv.Size = new System.Drawing.Size(227, 741);
            this.tv.TabIndex = 0;
            this.tv.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tv_AfterSelect);
            this.tv.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tv_NodeMouseClick);
            // 
            // ilTree
            // 
            this.ilTree.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilTree.ImageStream")));
            this.ilTree.TransparentColor = System.Drawing.Color.Transparent;
            this.ilTree.Images.SetKeyName(0, "folder");
            this.ilTree.Images.SetKeyName(1, "file");
            this.ilTree.Images.SetKeyName(2, "book");
            this.ilTree.Images.SetKeyName(3, "image");
            this.ilTree.Images.SetKeyName(4, "character");
            this.ilTree.Images.SetKeyName(5, "script");
            this.ilTree.Images.SetKeyName(6, "music");
            this.ilTree.Images.SetKeyName(7, "sound");
            this.ilTree.Images.SetKeyName(8, "font");
            this.ilTree.Images.SetKeyName(9, "cursor");
            this.ilTree.Images.SetKeyName(10, "palette");
            // 
            // cmsResource
            // 
            this.cmsResource.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsResource.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tcmiExportToFile});
            this.cmsResource.Name = "cmsTree";
            this.cmsResource.Size = new System.Drawing.Size(174, 28);
            // 
            // tcmiExportToFile
            // 
            this.tcmiExportToFile.Name = "tcmiExportToFile";
            this.tcmiExportToFile.Size = new System.Drawing.Size(173, 24);
            this.tcmiExportToFile.Text = "Export to file...";
            this.tcmiExportToFile.Click += new System.EventHandler(this.tcmiExportToFile_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbSave,
            this.tsbTranslated,
            this.tsbFind});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1053, 27);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbSave
            // 
            this.tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSave.Image = global::SCI_Translator.Properties.Resources.disk;
            this.tsbSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSave.Name = "tsbSave";
            this.tsbSave.Size = new System.Drawing.Size(29, 24);
            this.tsbSave.Text = "Save";
            this.tsbSave.Click += new System.EventHandler(this.tsbSave_Click);
            // 
            // tsbTranslated
            // 
            this.tsbTranslated.Checked = true;
            this.tsbTranslated.CheckOnClick = true;
            this.tsbTranslated.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsbTranslated.Image = global::SCI_Translator.Properties.Resources.font;
            this.tsbTranslated.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbTranslated.Name = "tsbTranslated";
            this.tsbTranslated.Size = new System.Drawing.Size(102, 24);
            this.tsbTranslated.Text = "Translated";
            this.tsbTranslated.CheckedChanged += new System.EventHandler(this.tsbTranslated_CheckedChanged);
            // 
            // tsbFind
            // 
            this.tsbFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbFind.Image = global::SCI_Translator.Properties.Resources.find;
            this.tsbFind.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFind.Name = "tsbFind";
            this.tsbFind.Size = new System.Drawing.Size(29, 24);
            this.tsbFind.Text = "Find";
            this.tsbFind.Click += new System.EventHandler(this.tsbFind_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslResourceInfo});
            this.statusStrip1.Location = new System.Drawing.Point(0, 768);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1053, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsslResourceInfo
            // 
            this.tsslResourceInfo.Name = "tsslResourceInfo";
            this.tsslResourceInfo.Size = new System.Drawing.Size(0, 16);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1053, 790);
            this.Controls.Add(this.sc);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormMain";
            this.Text = "SCI Translator";
            this.sc.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sc)).EndInit();
            this.sc.ResumeLayout(false);
            this.cmsResource.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}

