using Microsoft.Win32;
using SCI_Lib;
using System;
using System.Text;
using System.Windows.Forms;

namespace SCI_Translator
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string gameDir, translateDir;
            Encoding enc = null;

            if (args.Length == 0)
            {
                var form = new FormSelectDir();

                RegistryKey key = Registry.CurrentUser.CreateSubKey("SCI_Translator");
                form.GameDir = key.GetValue("LastGameDir")?.ToString();
                form.TranslateDir = key.GetValue("LastTranslateDir")?.ToString();

                var lastEnc = key.GetValue("LastEncoding");
                if (lastEnc != null)
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    form.GameEncoding = int.Parse(lastEnc.ToString());
                }
                key.Close();


                if (form.ShowDialog() != DialogResult.OK) return;

                gameDir = form.GameDir;
                translateDir = form.TranslateDir;
                if (translateDir.Length == 0) translateDir = null;

                enc = Encoding.GetEncoding(form.GameEncoding);
            }
            else
            {
                gameDir = args[0];
                translateDir = args.Length > 1 ? args[1] : null;
                if (args.Length > 2)
                    enc = Encoding.GetEncoding(int.Parse(args[2]));
            }

            SCIPackage package;
            SCIPackage translate;
            try
            {
                package = SCIPackage.Load(gameDir, enc);
                translate = !String.IsNullOrEmpty(translateDir) ? SCIPackage.Load(translateDir, enc) : null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            Application.Run(new FormMain(package, translate));
        }
    }
}
