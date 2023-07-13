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
        private ToolStripButton tsbGenOutline;
        private System.ComponentModel.IContainer components;
        private ToolStripButton tsbMirror;
        int _copyInd = -1;

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
                    bool mirror = false;
                    switch (GameEncoding.AllChars[i])
                    {
                        case 'А': or = 'A'; break;
                        case 'Б': or = 'B'; break;
                        case 'В': or = 'B'; break;
                        case 'Г': or = 'F'; break;
                        case 'Е': or = 'E'; break;
                        case 'Ё': or = 'E'; break;
                        case 'Ж': or = 'W'; break;
                        case 'З': or = '3'; break;
                        case 'И': or = 'N'; mirror = true; break;
                        case 'Й': or = 'N'; mirror = true; break;
                        case 'К': or = 'K'; break;
                        case 'Л': or = 'N'; break;
                        case 'М': or = 'M'; break;
                        case 'Н': or = 'H'; break;
                        case 'О': or = 'O'; break;
                        case 'П': or = 'H'; break;
                        case 'Р': or = 'P'; break;
                        case 'С': or = 'C'; break;
                        case 'Т': or = 'T'; break;
                        case 'У': or = 'Y'; break;
                        case 'Х': or = 'X'; break;
                        case 'Ц': or = 'U'; break;
                        case 'Ч': or = 'N'; break;
                        case 'Ш': or = 'W'; break;
                        case 'Щ': or = 'W'; break;
                        case 'Ь': or = 'B'; break;
                        case 'Э': or = 'C'; mirror = true; break;
                        case 'Ю': or = 'I'; break;
                        case 'Я': or = 'R'; mirror = true; break;
                        case 'а': or = 'a'; break;
                        case 'б': or = 'b'; break;
                        case 'в': or = 'b'; break;
                        case 'г': or = 'z'; break;
                        case 'д': or = 'n'; mirror = true; break;
                        case 'е': or = 'e'; break;
                        case 'ё': or = 'e'; break;
                        case 'ж': or = 'w'; break;
                        case 'з': or = 'g'; break;
                        case 'и': or = 'u'; break;
                        case 'к': or = 'k'; break;
                        case 'л': or = 'n'; mirror = true; break;
                        case 'м': or = 'm'; break;
                        case 'н': or = 'n'; break;
                        case 'о': or = 'o'; break;
                        case 'п': or = 'n'; break;
                        case 'р': or = 'p'; break;
                        case 'с': or = 'c'; break;
                        case 'у': or = 'y'; break;
                        case 'ф': or = 'o'; break;
                        case 'х': or = 'x'; break;
                        case 'ц': or = 'u'; break;
                        case 'ш': or = 'w'; break;
                        case 'щ': or = 'w'; break;
                        case 'ь': or = 'o'; break;
                        case 'э': or = 'c'; mirror = true; break;
                        case 'ю': or = 'o'; break;
                        case 'я': or = 'a'; break;
                    }

                    SpriteFrame frm;

                    if (or != 0)
                    {
                        frm = new SpriteFrame(spr[(byte)or]);
                        if (mirror)
                            frm.MirrorHoriz();
                    }
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

                //Bitmap b = viewer.CreateBitmap(i);
                //ilChars.Images.Add(b);
                var img = spr.Frames[i].GetImage().Rescale(32);
                ilChars.Images.Add(img);

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

        private void tsbGenOutline_Click(object sender, EventArgs e)
        {
            var form = new FormSelectFont((ResFont)_tres);
            if (form.ShowDialog() == DialogResult.OK)
            {
                ((ResFont)_tres).GenerateOutline(form.SelectedFont, form.SelectedStartIndex, form.SelectedEndIndex);
                FillChars();
            }
        }

        private void tsbMirror_Click(object sender, EventArgs e)
        {
            viewer.CurrentFrame.MirrorHoriz();
            viewer.Redraw();
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            splitContainer1 = new SplitContainer();
            lvChars = new ListView();
            ilChars = new ImageList(components);
            plPic = new Panel();
            tsInstruments = new ToolStrip();
            tsControls = new ToolStrip();
            tscbChar = new ToolStripComboBox();
            tsbResize = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            tsbUndo = new ToolStripButton();
            tsbRedo = new ToolStripButton();
            tsbExchange = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripSeparator2 = new ToolStripSeparator();
            tscbScale = new ToolStripComboBox();
            toolStripSeparator4 = new ToolStripSeparator();
            tsbShiftLeft = new ToolStripButton();
            tsbShiftRight = new ToolStripButton();
            tsbShiftUp = new ToolStripButton();
            tsbShiftDown = new ToolStripButton();
            toolStripSeparator5 = new ToolStripSeparator();
            tsbCopy = new ToolStripButton();
            tsbPaste = new ToolStripButton();
            tsbGenOutline = new ToolStripButton();
            tsbMirror = new ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tsControls.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4, 5, 4, 5);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(lvChars);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(plPic);
            splitContainer1.Panel2.Controls.Add(tsInstruments);
            splitContainer1.Panel2.Controls.Add(tsControls);
            splitContainer1.Size = new Size(1645, 1115);
            splitContainer1.SplitterDistance = 563;
            splitContainer1.SplitterWidth = 6;
            splitContainer1.TabIndex = 0;
            // 
            // lvChars
            // 
            lvChars.Dock = DockStyle.Fill;
            lvChars.HideSelection = false;
            lvChars.LargeImageList = ilChars;
            lvChars.Location = new Point(0, 0);
            lvChars.Margin = new Padding(4, 5, 4, 5);
            lvChars.Name = "lvChars";
            lvChars.Size = new Size(1645, 563);
            lvChars.TabIndex = 0;
            lvChars.UseCompatibleStateImageBehavior = false;
            lvChars.ItemActivate += lvChars_ItemActivate;
            // 
            // ilChars
            // 
            ilChars.ColorDepth = ColorDepth.Depth8Bit;
            ilChars.ImageSize = new Size(32, 32);
            ilChars.TransparentColor = Color.Transparent;
            // 
            // plPic
            // 
            plPic.AutoScroll = true;
            plPic.BackColor = SystemColors.AppWorkspace;
            plPic.Dock = DockStyle.Fill;
            plPic.Location = new Point(0, 53);
            plPic.Margin = new Padding(4, 5, 4, 5);
            plPic.Name = "plPic";
            plPic.Size = new Size(1645, 493);
            plPic.TabIndex = 0;
            // 
            // tsInstruments
            // 
            tsInstruments.GripStyle = ToolStripGripStyle.Hidden;
            tsInstruments.ImageScalingSize = new Size(20, 20);
            tsInstruments.Location = new Point(0, 28);
            tsInstruments.Name = "tsInstruments";
            tsInstruments.Size = new Size(1645, 25);
            tsInstruments.TabIndex = 0;
            tsInstruments.Text = "toolStrip1";
            // 
            // tsControls
            // 
            tsControls.GripStyle = ToolStripGripStyle.Hidden;
            tsControls.ImageScalingSize = new Size(20, 20);
            tsControls.Items.AddRange(new ToolStripItem[] { tscbChar, tsbResize, toolStripSeparator1, tsbUndo, tsbRedo, tsbExchange, toolStripSeparator3, toolStripSeparator2, tscbScale, toolStripSeparator4, tsbShiftLeft, tsbShiftRight, tsbShiftUp, tsbShiftDown, toolStripSeparator5, tsbCopy, tsbPaste, tsbGenOutline, tsbMirror });
            tsControls.Location = new Point(0, 0);
            tsControls.Name = "tsControls";
            tsControls.Size = new Size(1645, 28);
            tsControls.TabIndex = 0;
            tsControls.Text = "toolStrip1";
            // 
            // tscbChar
            // 
            tscbChar.Name = "tscbChar";
            tscbChar.Size = new Size(99, 28);
            tscbChar.SelectedIndexChanged += tscbChar_SelectedIndexChanged;
            // 
            // tsbResize
            // 
            tsbResize.Image = Properties.Resources.arrow_inout;
            tsbResize.ImageTransparentColor = Color.Magenta;
            tsbResize.Name = "tsbResize";
            tsbResize.Size = new Size(75, 25);
            tsbResize.Text = "Resize";
            tsbResize.Click += tsbResize_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 28);
            // 
            // tsbUndo
            // 
            tsbUndo.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbUndo.Image = Properties.Resources.arrow_undo;
            tsbUndo.ImageTransparentColor = Color.Magenta;
            tsbUndo.Name = "tsbUndo";
            tsbUndo.Size = new Size(29, 25);
            tsbUndo.Text = "Undo";
            tsbUndo.Click += tsbUndo_Click;
            // 
            // tsbRedo
            // 
            tsbRedo.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbRedo.Image = Properties.Resources.arrow_redo;
            tsbRedo.ImageTransparentColor = Color.Magenta;
            tsbRedo.Name = "tsbRedo";
            tsbRedo.Size = new Size(29, 25);
            tsbRedo.Text = "Redo";
            tsbRedo.Click += tsbRedo_Click;
            // 
            // tsbExchange
            // 
            tsbExchange.Alignment = ToolStripItemAlignment.Right;
            tsbExchange.Image = Properties.Resources.arrow_out;
            tsbExchange.ImageTransparentColor = Color.Magenta;
            tsbExchange.Name = "tsbExchange";
            tsbExchange.Size = new Size(134, 25);
            tsbExchange.Text = "Exchange chars";
            tsbExchange.Click += tsbExchange_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 28);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 28);
            // 
            // tscbScale
            // 
            tscbScale.Name = "tscbScale";
            tscbScale.Size = new Size(99, 28);
            tscbScale.TextChanged += tscbScale_TextChanged;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 28);
            // 
            // tsbShiftLeft
            // 
            tsbShiftLeft.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbShiftLeft.Image = Properties.Resources.arrow_left;
            tsbShiftLeft.ImageTransparentColor = Color.Magenta;
            tsbShiftLeft.Name = "tsbShiftLeft";
            tsbShiftLeft.Size = new Size(29, 25);
            tsbShiftLeft.Text = "Shift left";
            tsbShiftLeft.Click += tsbShiftLeft_Click;
            // 
            // tsbShiftRight
            // 
            tsbShiftRight.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbShiftRight.Image = Properties.Resources.arrow_right;
            tsbShiftRight.ImageTransparentColor = Color.Magenta;
            tsbShiftRight.Name = "tsbShiftRight";
            tsbShiftRight.Size = new Size(29, 25);
            tsbShiftRight.Text = "Shift right";
            tsbShiftRight.Click += tsbShiftRight_Click;
            // 
            // tsbShiftUp
            // 
            tsbShiftUp.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbShiftUp.Image = Properties.Resources.arrow_up;
            tsbShiftUp.ImageTransparentColor = Color.Magenta;
            tsbShiftUp.Name = "tsbShiftUp";
            tsbShiftUp.Size = new Size(29, 25);
            tsbShiftUp.Text = "Shift up";
            tsbShiftUp.Click += tsbShiftUp_Click;
            // 
            // tsbShiftDown
            // 
            tsbShiftDown.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbShiftDown.Image = Properties.Resources.arrow_down;
            tsbShiftDown.ImageTransparentColor = Color.Magenta;
            tsbShiftDown.Name = "tsbShiftDown";
            tsbShiftDown.Size = new Size(29, 25);
            tsbShiftDown.Text = "Shift down";
            tsbShiftDown.Click += tsbShiftDown_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(6, 28);
            // 
            // tsbCopy
            // 
            tsbCopy.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbCopy.Image = Properties.Resources.copy_edit;
            tsbCopy.ImageTransparentColor = Color.Magenta;
            tsbCopy.Name = "tsbCopy";
            tsbCopy.Size = new Size(29, 25);
            tsbCopy.Text = "Copy character";
            tsbCopy.Click += tsbCopy_Click;
            // 
            // tsbPaste
            // 
            tsbPaste.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbPaste.Image = Properties.Resources.paste_edit;
            tsbPaste.ImageTransparentColor = Color.Magenta;
            tsbPaste.Name = "tsbPaste";
            tsbPaste.Size = new Size(29, 25);
            tsbPaste.Text = "Paste character";
            tsbPaste.Click += tsbPaste_Click;
            // 
            // tsbGenOutline
            // 
            tsbGenOutline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbGenOutline.Image = Properties.Resources.type;
            tsbGenOutline.ImageTransparentColor = Color.Magenta;
            tsbGenOutline.Name = "tsbGenOutline";
            tsbGenOutline.Size = new Size(29, 25);
            tsbGenOutline.Text = "Generate outline";
            tsbGenOutline.Click += tsbGenOutline_Click;
            // 
            // tsbMirror
            // 
            tsbMirror.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbMirror.Image = Properties.Resources.mirror_horizontally;
            tsbMirror.ImageTransparentColor = Color.Magenta;
            tsbMirror.Name = "tsbMirror";
            tsbMirror.Size = new Size(29, 25);
            tsbMirror.Text = "toolStripButton1";
            tsbMirror.Click += tsbMirror_Click;
            // 
            // FontView
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            Controls.Add(splitContainer1);
            Margin = new Padding(4, 5, 4, 5);
            Name = "FontView";
            Size = new Size(1645, 1115);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tsControls.ResumeLayout(false);
            tsControls.PerformLayout();
            ResumeLayout(false);
        }
    }
}
