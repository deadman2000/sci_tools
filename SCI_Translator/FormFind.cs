using SCI_Lib;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SCI_Translator
{
    partial class FormFind : Form
    {
        private readonly SCIPackage _package;
        private readonly SCIPackage _translate;
        private readonly FormMain _main;

        string _pattern;
        bool _exact;
        bool _matchCase;

        public FormFind(FormMain main, SCIPackage package, SCIPackage translate)
        {
            InitializeComponent();
            _package = package;
            _translate = translate;
            _main = main;
        }

        private void btFind_Click(object sender, EventArgs e)
        {
            lbResults.Items.Clear();

            _exact = chbExact.Checked;
            _matchCase = chbMatchCase.Checked;

            _pattern = tbPattern.Text;
            if (!_matchCase)
                _pattern = _pattern.ToLower();

            foreach (var txt in _package.Texts)
                try
                {
                    FindText(txt);
                }
                catch { }

            foreach (var scr in _package.Scripts)
                try
                {
                    FindScript(scr);
                }
                catch { }

            foreach (var msg in _package.Messages)
                try
                {
                    FindMessage(msg);
                }
                catch { }
        }

        private void FindText(ResText txt)
        {
            var lines = txt.GetStrings();
            string[] tr = null;
            if (_translate != null)
                tr = _translate.Get(txt).GetStrings();
            for (int i = 0; i < lines.Length; i++)
            {
                if (IsPass(lines[i]))
                    AddResult(txt, i, lines[i]);
                else if (tr != null && IsPass(tr[i]))
                    AddResult(txt, i, tr[i]);
            }
        }

        private void FindScript(ResScript res)
        {
            var script = res.GetScript();
            if (script == null) return;

            var sections = script.Get<StringSection>();
            if (sections == null) return;

            List<StringSection> trSections = null;
            if (_translate != null)
            {
                var tr = _translate.Get(res).GetScript();
                trSections = tr.Get<StringSection>();
            }

            for (int i = 0; i < sections.Count; i++)
            {
                var sec = sections[i];
                for (int ind = 0; ind < sec.Strings.Count; ind++)
                {
                    StringConst str = sec.Strings[ind];
                    if (IsPass(str.Value))
                        AddResult(res, ind, str.Value);
                    else if (trSections != null && IsPass(trSections[i].Strings[ind].Value))
                        AddResult(res, ind, trSections[i].Strings[ind].Value);
                }
            }
        }

        private void FindMessage(ResMessage msg)
        {
            var messages = msg.GetMessages();
            List<MessageRecord> tr = null;
            if (_translate != null)
                tr = _translate.Get(msg).GetMessages();

            for (int i = 0; i < messages.Count; i++)
            {
                if (IsPass(messages[i].Text))
                    AddResult(msg, i, messages[i].Text);
                else if (tr != null && IsPass(tr[i].Text))
                    AddResult(msg, i, tr[i].Text);
            }
        }

        private bool IsPass(string txt)
        {
            if (!_matchCase)
                txt = txt.ToLower();

            if (_exact)
                return txt.Equals(_pattern);

            return txt.Contains(_pattern);
        }

        private void AddResult(Resource res, int ind, string txt)
        {
            lbResults.Items.Add(new SearchResult(res.FileName, ind, txt));
        }

        class SearchResult
        {
            public string FileName { get; }

            public int Index { get; }

            public string Value { get; }

            public SearchResult(string fileName, int index, string value)
            {
                FileName = fileName;
                Index = index;
                Value = value;
            }

            public override string ToString() => $"{FileName}\t{Index}\t{Value}";
        }

        private void lbResults_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lbResults.SelectedItem != null)
            {
                SearchResult sr = (SearchResult)lbResults.SelectedItem;
                _main.SelectFile(sr.FileName, sr.Index);
            }
        }

        private void FormFind_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void FormFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }
    }
}
