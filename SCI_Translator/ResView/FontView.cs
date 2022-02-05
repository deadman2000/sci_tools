using SCI_Lib.Pictures;
using SCI_Lib.Resources;
using SCI_Translator.ImageEditor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SCI_Translator.ResView
{
    class FontView : ResViewer
    {
        private readonly PixelPictureViewer viewer;
        private readonly PixelPalette pal;
        private SCIFont spr;

        private Panel plPic;
        private ListView lvChars;
        private ImageList ilChars;
        private ToolStripButton tsbUndo;
        private ToolStripButton tsbRedo;
        private ToolStrip tsInstruments;
        private ToolStripButton tsbExchange;
        private ToolStripComboBox tscbChar;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton tsbResize;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripComboBox tscbScale;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton tsbShiftLeft;
        private ToolStripButton tsbShiftRight;
        private ToolStripButton tsbShiftUp;
        private ToolStripButton tsbShiftDown;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripButton tsbCopy;
        private ToolStripButton tsbPaste;
        private SplitContainer splitContainer1;
        private ToolStrip tsControls;
        private ToolStripSeparator toolStripSeparator1;
        private System.ComponentModel.IContainer components;

        public FontView()
        {
            InitializeComponent();

            for (int i = 1; i <= 20; i++)
                tscbScale.Items.Add(i);
            tscbScale.Text = "20";

            pal = new PixelPalette();
            pal[0] = Color.White;
            pal[1] = Color.Black;

            viewer = new PixelPictureViewer();
            viewer.Zoom = 20;
            viewer.Palette = pal;
            plPic.Controls.Add(viewer);
            viewer.Parent = plPic;
            viewer.Anchor = AnchorStyles.None;

            viewer.Click += viewer_Click;
            viewer.MouseDown += viewer_MouseDown;
            viewer.MouseLeave += viewer_MouseLeave;
            viewer.MouseMove += viewer_MouseMove;
            viewer.MouseUp += viewer_MouseUp;

            AddInstrument(new InstrPencil(1, 1, 0));
            ((ToolStripButton)tsInstruments.Items[0]).Checked = true;
        }

        protected override void Reload()
        {
            var font = (ResFont)Current;

            _copyInd = -1;
            viewer.Sprite = spr = font.GetFont();

            FillChars();
            UpdateView();
        }

        protected override void SaveContent()
        {
            ((ResFont)_tres).SetFont(spr);
            _tres.SavePatch();
        }

        #region Editing

        void viewer_Click(object sender, EventArgs e)
        {
            plPic.Focus();
        }

        void viewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.X >= viewer.Width || e.Y >= viewer.Height || e.X < 0 || e.Y < 0) return;
            if (_instr != null) _instr.OnMouseDown(e);
        }

        void viewer_MouseLeave(object sender, EventArgs e)
        {
            if (_instr != null) _instr.OnMouseLeave();
        }

        void viewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X >= viewer.Width || e.Y >= viewer.Height || e.X < 0 || e.Y < 0)
            {
                if (_instr != null) _instr.OnMouseLeave();
                return;
            }

            Point p = viewer.PointToPicture(e.Location);

            if (_instr != null) _instr.OnMouseMove(e);
        }

        void viewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (_instr != null) _instr.OnMouseUp(e);

            viewer.CommitEdit();
            UpdateUndoRedo();
        }

        private void tsbUndo_Click(object sender, EventArgs e)
        {
            Undo();
        }

        private void tsbRedo_Click(object sender, EventArgs e)
        {
            Redo();
        }

        public void Undo()
        {
            viewer.Undo();
            UpdateUndoRedo();
        }

        public void Redo()
        {
            viewer.Redo();
            UpdateUndoRedo();
        }

        private void UpdateUndoRedo()
        {
            tsbUndo.Enabled = viewer.HasUndo;
            tsbRedo.Enabled = viewer.HasRedo;
        }

        readonly List<ToolStripButton> _instrumentButtons = new List<ToolStripButton>();
        void AddInstrument(BaseInstrument instr)
        {
            instr.Pic = viewer;

            ToolStripButton tsb = new ToolStripButton(instr.Name, instr.Image);
            tsb.Tag = instr;
            tsb.CheckOnClick = true;
            tsb.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsb.CheckedChanged += new EventHandler(tsbInstr_CheckedChanged);

            tsInstruments.Items.Add(tsb);
            _instrumentButtons.Add(tsb);
        }

        ToolStripButton _currentInstrumentButton;
        void tsbInstr_CheckedChanged(object sender, EventArgs e)
        {
            ToolStripButton tsb = (ToolStripButton)sender;
            if (tsb.Checked)
            {
                if (_currentInstrumentButton == tsb) return;

                _currentInstrumentButton = tsb;
                SelectInstrument((BaseInstrument)tsb.Tag);
            }
            else
            {
                if (_currentInstrumentButton == tsb)
                    tsb.Checked = true;
            }
        }

        BaseInstrument _instr = null;

        public void SelectInstrument(BaseInstrument instr)
        {
            if (_instr == instr) return;

            if (_instr != null)
                _instr.Deactivate();

            _instr = instr;
            if (_instr.Cursor != null)
                viewer.Cursor = _instr.Cursor;
            else
                viewer.Cursor = Cursors.Default;

            foreach (ToolStripButton b in _instrumentButtons)
            {
                if (b.Tag != instr && b.Checked)
                    b.Checked = false;
                else if (b.Tag == instr && !b.Checked)
                    b.Checked = true;
            }

            viewer.CurrInstrument = _instr;
        }


        private void tsbExchange_Click(object sender, EventArgs e)
        {
            if (spr.Frames.Count <= 0x80)
            {
                for (int i = 0x80; i <= 0xF3; i++)
                {
                    char or = (char)0;
                    switch (GameEncoding.AllChars[i])
                    {
                        case 'а': or = 'a'; break;
                        case 'А': or = 'A'; break;
                        case 'В': or = 'B'; break;
                        case 'е': or = 'e'; break;
                        case 'Е': or = 'E'; break;
                        case 'К': or = 'K'; break;
                        case 'м': or = 'm'; break;
                        case 'М': or = 'M'; break;
                        case 'Н': or = 'H'; break;
                        case 'о': or = 'o'; break;
                        case 'О': or = 'O'; break;
                        case 'п': or = 'n'; break;
                        case 'р': or = 'p'; break;
                        case 'Р': or = 'P'; break;
                        case 'с': or = 'c'; break;
                        case 'С': or = 'C'; break;
                        case 'Т': or = 'T'; break;
                        case 'у': or = 'y'; break;
                        case 'У': or = 'Y'; break;
                        case 'х': or = 'x'; break;
                        case 'Х': or = 'X'; break;
                        case 'ш': or = 'w'; break;
                        case 'Ш': or = 'W'; break;
                        case 'щ': or = 'w'; break;
                        case 'Щ': or = 'W'; break;
                        case 'ж': or = 'w'; break;
                        case 'Ж': or = 'W'; break;
                        case 'ё': or = 'e'; break;
                        case 'Ё': or = 'E'; break;
                        case 'И': or = 'N'; break;
                        case 'Й': or = 'N'; break;
                        case 'Э': or = 'G'; break;
                        case 'Я': or = 'R'; break;
                        case 'и': or = 'u'; break;
                        case 'к': or = 'k'; break;
                    }

                    SpriteFrame frm;

                    if (or != 0)
                        frm = new SpriteFrame(spr[(byte)or]);
                    else
                        frm = new SpriteFrame(1, 1);

                    spr.Frames.Add(frm);
                }

                FillChars();
                //btExchange.Visible = false;
            }
            else
            {
                if (spr.Frames.Count > 0xF2)
                {
                    while (spr.Frames.Count > 0xF2) spr.Frames.RemoveAt(spr.Frames.Count - 1);
                    FillChars();
                }
            }
        }

        #endregion

        void Centering()
        {
            viewer.Location = new Point(plPic.Width / 2 - viewer.Width / 2, plPic.Height / 2 - viewer.Height / 2);
        }

        void FillChars()
        {
            tscbChar.Items.Clear();
            lvChars.Items.Clear();
            ilChars.Images.Clear();

            for (int i = 0; i < spr.Frames.Count; i++)
            {
                string name = String.Format("0x{0:X2} {1}", i, GameEncoding.AllChars[i]);
                tscbChar.Items.Add(name);

                Bitmap b = viewer.CreateBitmap(i);
                ilChars.Images.Add(b);

                ListViewItem lvi = lvChars.Items.Add(name);
                lvi.Tag = i;
                lvi.ImageIndex = i;
            }

            if (tscbChar.Items.Count > viewer.CurrentFrameIndex)
                tscbChar.SelectedIndex = viewer.CurrentFrameIndex;
        }

        void UpdateView()
        {
            if (viewer.CurrentFrame != null)
                tsbResize.Text = String.Format("{0}x{1}", viewer.CurrentFrame.Width, viewer.CurrentFrame.Height);
            viewer.Redraw();
            Centering();
        }

        private void tscbChar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tscbChar.SelectedIndex >= 0 && tscbChar.SelectedIndex < spr.Frames.Count)
            {
                viewer.CurrentFrameIndex = tscbChar.SelectedIndex;
                UpdateView();
            }
        }

        private void lvChars_ItemActivate(object sender, EventArgs e)
        {
            if (lvChars.SelectedItems.Count > 0)
            {
                int i = (int)lvChars.SelectedItems[0].Tag;
                tscbChar.SelectedIndex = i;
            }
        }

        private void tsbResize_Click(object sender, EventArgs e)
        {
            NewSizeDialog nsdlg = new NewSizeDialog(viewer.CurrentFrame.Width, viewer.CurrentFrame.Height);
            if (nsdlg.ShowDialog() == DialogResult.OK)
            {
                viewer.CurrentFrame.Resize(nsdlg.SWidth, nsdlg.SHeight);
                UpdateView();
            }
        }


        private void tscbScale_TextChanged(object sender, EventArgs e)
        {
            if (viewer == null) return;

            if (int.TryParse(tscbScale.Text, out int scale) && scale > 0)
            {
                viewer.Zoom = scale;
                Centering();
            }
        }

        private void tsbShiftLeft_Click(object sender, EventArgs e)
        {
            viewer.CurrentFrame.ShiftLeft(0);
            viewer.Redraw();
        }

        private void tsbShiftRight_Click(object sender, EventArgs e)
        {
            viewer.CurrentFrame.ShiftRight(0);
            viewer.Redraw();
        }

        private void tsbShiftUp_Click(object sender, EventArgs e)
        {
            viewer.CurrentFrame.ShiftUp(0);
            viewer.Redraw();
        }

        private void tsbShiftDown_Click(object sender, EventArgs e)
        {
            viewer.CurrentFrame.ShiftDown(0);
            viewer.Redraw();
        }

        int _copyInd = -1;
        private void tsbCopy_Click(object sender, EventArgs e)
        {
            _copyInd = viewer.CurrentFrameIndex;
        }

        private void tsbPaste_Click(object sender, EventArgs e)
        {
            if (_copyInd == -1) return;
            spr.Frames[viewer.CurrentFrameIndex] = new SpriteFrame(spr[_copyInd]);
            UpdateView();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lvChars = new System.Windows.Forms.ListView();
            this.ilChars = new System.Windows.Forms.ImageList(this.components);
            this.plPic = new System.Windows.Forms.Panel();
            this.tsInstruments = new System.Windows.Forms.ToolStrip();
            this.tsControls = new System.Windows.Forms.ToolStrip();
            this.tscbChar = new System.Windows.Forms.ToolStripComboBox();
            this.tsbResize = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbUndo = new System.Windows.Forms.ToolStripButton();
            this.tsbRedo = new System.Windows.Forms.ToolStripButton();
            this.tsbExchange = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tscbScale = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbShiftLeft = new System.Windows.Forms.ToolStripButton();
            this.tsbShiftRight = new System.Windows.Forms.ToolStripButton();
            this.tsbShiftUp = new System.Windows.Forms.ToolStripButton();
            this.tsbShiftDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbCopy = new System.Windows.Forms.ToolStripButton();
            this.tsbPaste = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tsControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lvChars);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.plPic);
            this.splitContainer1.Panel2.Controls.Add(this.tsInstruments);
            this.splitContainer1.Panel2.Controls.Add(this.tsControls);
            this.splitContainer1.Size = new System.Drawing.Size(1446, 1088);
            this.splitContainer1.SplitterDistance = 552;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 0;
            // 
            // lvChars
            // 
            this.lvChars.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvChars.HideSelection = false;
            this.lvChars.LargeImageList = this.ilChars;
            this.lvChars.Location = new System.Drawing.Point(0, 0);
            this.lvChars.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lvChars.Name = "lvChars";
            this.lvChars.Size = new System.Drawing.Size(1446, 552);
            this.lvChars.TabIndex = 0;
            this.lvChars.UseCompatibleStateImageBehavior = false;
            this.lvChars.ItemActivate += new System.EventHandler(this.lvChars_ItemActivate);
            // 
            // ilChars
            // 
            this.ilChars.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ilChars.ImageSize = new System.Drawing.Size(16, 16);
            this.ilChars.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // plPic
            // 
            this.plPic.AutoScroll = true;
            this.plPic.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.plPic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plPic.Location = new System.Drawing.Point(0, 53);
            this.plPic.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.plPic.Name = "plPic";
            this.plPic.Size = new System.Drawing.Size(1446, 477);
            this.plPic.TabIndex = 0;
            // 
            // tsInstruments
            // 
            this.tsInstruments.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsInstruments.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.tsInstruments.Location = new System.Drawing.Point(0, 28);
            this.tsInstruments.Name = "tsInstruments";
            this.tsInstruments.Size = new System.Drawing.Size(1446, 25);
            this.tsInstruments.TabIndex = 0;
            this.tsInstruments.Text = "toolStrip1";
            // 
            // tsControls
            // 
            this.tsControls.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsControls.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.tsControls.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tscbChar,
            this.tsbResize,
            this.toolStripSeparator1,
            this.tsbUndo,
            this.tsbRedo,
            this.tsbExchange,
            this.toolStripSeparator3,
            this.toolStripSeparator2,
            this.tscbScale,
            this.toolStripSeparator4,
            this.tsbShiftLeft,
            this.tsbShiftRight,
            this.tsbShiftUp,
            this.tsbShiftDown,
            this.toolStripSeparator5,
            this.tsbCopy,
            this.tsbPaste});
            this.tsControls.Location = new System.Drawing.Point(0, 0);
            this.tsControls.Name = "tsControls";
            this.tsControls.Size = new System.Drawing.Size(1446, 28);
            this.tsControls.TabIndex = 0;
            this.tsControls.Text = "toolStrip1";
            // 
            // tscbChar
            // 
            this.tscbChar.Name = "tscbChar";
            this.tscbChar.Size = new System.Drawing.Size(99, 28);
            this.tscbChar.SelectedIndexChanged += new System.EventHandler(this.tscbChar_SelectedIndexChanged);
            // 
            // tsbResize
            // 
            this.tsbResize.Image = global::SCI_Translator.Properties.Resources.arrow_inout;
            this.tsbResize.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbResize.Name = "tsbResize";
            this.tsbResize.Size = new System.Drawing.Size(75, 25);
            this.tsbResize.Text = "Resize";
            this.tsbResize.Click += new System.EventHandler(this.tsbResize_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            // 
            // tsbUndo
            // 
            this.tsbUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbUndo.Image = global::SCI_Translator.Properties.Resources.arrow_undo;
            this.tsbUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUndo.Name = "tsbUndo";
            this.tsbUndo.Size = new System.Drawing.Size(29, 25);
            this.tsbUndo.Text = "Undo";
            this.tsbUndo.Click += new System.EventHandler(this.tsbUndo_Click);
            // 
            // tsbRedo
            // 
            this.tsbRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRedo.Image = global::SCI_Translator.Properties.Resources.arrow_redo;
            this.tsbRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRedo.Name = "tsbRedo";
            this.tsbRedo.Size = new System.Drawing.Size(29, 25);
            this.tsbRedo.Text = "Redo";
            this.tsbRedo.Click += new System.EventHandler(this.tsbRedo_Click);
            // 
            // tsbExchange
            // 
            this.tsbExchange.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbExchange.Image = global::SCI_Translator.Properties.Resources.arrow_out;
            this.tsbExchange.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbExchange.Name = "tsbExchange";
            this.tsbExchange.Size = new System.Drawing.Size(134, 25);
            this.tsbExchange.Text = "Exchange chars";
            this.tsbExchange.Click += new System.EventHandler(this.tsbExchange_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
            // 
            // tscbScale
            // 
            this.tscbScale.Name = "tscbScale";
            this.tscbScale.Size = new System.Drawing.Size(99, 28);
            this.tscbScale.TextChanged += new System.EventHandler(this.tscbScale_TextChanged);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 28);
            // 
            // tsbShiftLeft
            // 
            this.tsbShiftLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbShiftLeft.Image = global::SCI_Translator.Properties.Resources.arrow_left;
            this.tsbShiftLeft.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbShiftLeft.Name = "tsbShiftLeft";
            this.tsbShiftLeft.Size = new System.Drawing.Size(29, 25);
            this.tsbShiftLeft.Text = "Shift left";
            this.tsbShiftLeft.Click += new System.EventHandler(this.tsbShiftLeft_Click);
            // 
            // tsbShiftRight
            // 
            this.tsbShiftRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbShiftRight.Image = global::SCI_Translator.Properties.Resources.arrow_right;
            this.tsbShiftRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbShiftRight.Name = "tsbShiftRight";
            this.tsbShiftRight.Size = new System.Drawing.Size(29, 25);
            this.tsbShiftRight.Text = "Shift right";
            this.tsbShiftRight.Click += new System.EventHandler(this.tsbShiftRight_Click);
            // 
            // tsbShiftUp
            // 
            this.tsbShiftUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbShiftUp.Image = global::SCI_Translator.Properties.Resources.arrow_up;
            this.tsbShiftUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbShiftUp.Name = "tsbShiftUp";
            this.tsbShiftUp.Size = new System.Drawing.Size(29, 25);
            this.tsbShiftUp.Text = "Shift up";
            this.tsbShiftUp.Click += new System.EventHandler(this.tsbShiftUp_Click);
            // 
            // tsbShiftDown
            // 
            this.tsbShiftDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbShiftDown.Image = global::SCI_Translator.Properties.Resources.arrow_down;
            this.tsbShiftDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbShiftDown.Name = "tsbShiftDown";
            this.tsbShiftDown.Size = new System.Drawing.Size(29, 25);
            this.tsbShiftDown.Text = "Shift down";
            this.tsbShiftDown.Click += new System.EventHandler(this.tsbShiftDown_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 28);
            // 
            // tsbCopy
            // 
            this.tsbCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCopy.Image = global::SCI_Translator.Properties.Resources.copy_edit;
            this.tsbCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCopy.Name = "tsbCopy";
            this.tsbCopy.Size = new System.Drawing.Size(29, 25);
            this.tsbCopy.Text = "Copy character";
            this.tsbCopy.Click += new System.EventHandler(this.tsbCopy_Click);
            // 
            // tsbPaste
            // 
            this.tsbPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPaste.Image = global::SCI_Translator.Properties.Resources.paste_edit;
            this.tsbPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPaste.Name = "tsbPaste";
            this.tsbPaste.Size = new System.Drawing.Size(29, 25);
            this.tsbPaste.Text = "Paste character";
            this.tsbPaste.Click += new System.EventHandler(this.tsbPaste_Click);
            // 
            // FontView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FontView";
            this.Size = new System.Drawing.Size(1446, 1088);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tsControls.ResumeLayout(false);
            this.tsControls.PerformLayout();
            this.ResumeLayout(false);

        }

    }
}
