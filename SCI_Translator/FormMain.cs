using SCI_Lib;
using SCI_Lib.Resources;
using SCI_Translator.ResView;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SCI_Translator
{
    partial class FormMain : Form
    {
        private readonly SCIPackage _package;
        private readonly SCIPackage _translate;

        private readonly HexViewer hexViewer;
        private readonly TextViewer textViewer;
        private readonly FontView fontView;
        private readonly ScriptView scriptView;
        private readonly VocabView vocabView;
        private readonly WordsView wordsView;
        private readonly SuffixesView suffixesView;
        private readonly MsgView msgView;
        private readonly PicView picView;

        private int? SelectRow;
        private ResViewer _currentViewer;

        public FormMain(SCIPackage package, SCIPackage translate)
        {
            _package = package;
            _translate = translate;

            InitializeComponent();

            Text = package.GameDirectory;

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            sc.Panel2.Controls.Add(hexViewer = new HexViewer());
            sc.Panel2.Controls.Add(textViewer = new TextViewer());
            sc.Panel2.Controls.Add(fontView = new FontView());
            sc.Panel2.Controls.Add(scriptView = new ScriptView());
            sc.Panel2.Controls.Add(vocabView = new VocabView());
            sc.Panel2.Controls.Add(wordsView = new WordsView());
            sc.Panel2.Controls.Add(suffixesView = new SuffixesView());
            sc.Panel2.Controls.Add(msgView = new MsgView());
            sc.Panel2.Controls.Add(picView = new PicView());

            foreach (ResType resType in Enum.GetValues(typeof(ResType)))
            {
                var resources = package.GetResources(resType);
                if (!resources.Any()) continue;

                TreeNode tnRes = tv.Nodes.Add(ResTypeName(resType));
                tnRes.ImageKey = "folder";
                tnRes.SelectedImageKey = tnRes.ImageKey;

                foreach (var res in resources)
                {
                    TreeNode tnRec = tnRes.Nodes.Add(res.ToString());
                    tnRec.ImageKey = ResTypeKey(resType);
                    tnRec.SelectedImageKey = tnRec.ImageKey;
                    tnRec.Tag = res;
                }
            }

            if (_translate == null)
            {
                tsbTranslated.Checked = false;
                tsbTranslated.Visible = false;
                tsbSave.Visible = false;
            }
        }

        string ResTypeName(ResType type)
        {
            switch (type)
            {
                case ResType.AudioPath: return "Audio path";
                case ResType.CDAudio: return "CD Audio";
                default: return type.ToString();
            }
        }

        string ResTypeKey(ResType type)
        {
            switch (type)
            {
                case ResType.View: return "character";
                case ResType.Picture: return "image";
                case ResType.Script: return "script";
                case ResType.Text: return "book";
                case ResType.Sound: return "music";
                case ResType.Audio:
                case ResType.AudioPath:
                case ResType.CDAudio: return "sound";
                case ResType.Font: return "font";
                case ResType.Cursor: return "cursor";
                case ResType.Palette: return "palette";
                default: return "file";
            }
        }

        private void tv_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Resource resource)
            {
                ShowResource(resource, tsbTranslated.Checked);
            }
            else
            {
                tsslResourceInfo.Text = "";
            }
        }

        private void ShowResource(Resource res, bool translated)
        {
            var info = res.GetInfo();
            tsslResourceInfo.Text = String.Format("{0}  {1} ({2:X8}h)  {3}", res.Type, res.Volumes[0].FileName, res.Volumes[0].Offset, info);

            if (_currentViewer != null)
                _currentViewer.Save();

            _currentViewer = GetViewer(res);

            Resource tres = _translate?.Get(res);

            _currentViewer.Activate(res, tres, translated);
            if (SelectRow.HasValue)
            {
                _currentViewer.FocusRow(SelectRow.Value);
                SelectRow = null;
            }
            _currentViewer.BringToFront();
        }

        private ResViewer GetViewer(Resource res)
        {
            switch (res.Type)
            {
                case ResType.Text: return textViewer;
                case ResType.Font: return fontView;
                case ResType.Script: return scriptView;
                case ResType.Vocabulary:
                    if (res.Number == 0) return wordsView;
                    if (res.Number == 901) return suffixesView;
                    return vocabView;
                case ResType.Message: return msgView;
                case ResType.Picture: return picView;
                default: return hexViewer;
            }
        }

        private void tsbSave_Click(object sender, EventArgs e)
        {
            if (_currentViewer != null)
                _currentViewer.Save();
        }

        private void tsbTranslated_CheckedChanged(object sender, EventArgs e)
        {
            if (_currentViewer != null && _currentViewer.DiffTranslate)
            {
                _currentViewer.Save();
                ShowResource(_currentViewer.Resource, tsbTranslated.Checked);
            }

            tsbSave.Enabled = tsbTranslated.Checked;
        }

        private void tsbFind_Click(object sender, EventArgs e)
        {
            FormFind fmFind = new FormFind(this, _package, _translate);
            fmFind.Show();
        }

        public void SelectFile(string fileName, int rowNum)
        {
            if (_currentViewer != null && _currentViewer.Resource.FileName == fileName)
            {
                _currentViewer.FocusRow(rowNum);
                return;
            }

            foreach (TreeNode tn in tv.Nodes)
            {
                foreach (TreeNode tnFile in tn.Nodes)
                {
                    if (tnFile.Text.Equals(fileName))
                    {
                        if (tv.SelectedNode == tnFile)
                        {
                            _currentViewer.FocusRow(rowNum);
                        }
                        else
                        {
                            SelectRow = rowNum;
                            tv.SelectedNode = tnFile;
                        }
                        return;
                    }
                }
            }
        }

        private void tcmiExportToFile_Click(object sender, EventArgs e)
        {
            if (tv.SelectedNode == null) return;
            Resource res = tv.SelectedNode.Tag as Resource;

            if (tsbTranslated.Checked)
            {
                res = _translate.Get(res);
            }

            var bytes = res.GetContent();
            File.WriteAllBytes(res.FileName, bytes);
        }

        private void tv_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                tv.SelectedNode = e.Node;
                if (e.Node.Tag is Resource)
                {
                    cmsResource.Show(tv, e.Location);
                }
            }
        }
    }
}
