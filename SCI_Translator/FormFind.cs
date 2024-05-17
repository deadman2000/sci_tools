using SCI_Lib;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var strings = script.AllStrings().ToArray();
            if (strings == null) return;

            StringConst[] trStrings = null;
            if (_translate != null)
            {
                var tr = _translate.Get(res).GetScript();
                trStrings = tr.AllStrings().ToArray();
            }

            for (int i = 0; i < strings.Length; i++)
            {
                var str = strings[i];
                if (IsPass(str.Value))
                    AddResult(res, i, str.Value);
                else if (trStrings != null && IsPass(trStrings[i].Value))
                    AddResult(res, i, trStrings[i].Value);
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
