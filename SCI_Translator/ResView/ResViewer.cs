using SCI_Lib;
using SCI_Lib.Resources;
using System.Windows.Forms;

namespace SCI_Translator.ResView
{
    class ResViewer : UserControl
    {

        protected Resource _res;
        protected Resource _tres;

        protected bool _translated;

        public Resource Current => _translated ? _tres : _res;

        public Resource Resource { get { return _res; } }

        public virtual bool IsAutoSave => false;

        public void Save()
        {
            if (!DiffTranslate || _translated)
                SaveContent();
        }

        protected virtual void SaveContent()
        {
        }

        public GameEncoding GameEncoding => Resource.Package.GameEncoding;

        public ResViewer()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
        }

        /// <summary>
        /// true - форма показывает только один вариант (исходный/перевод)
        /// false - на форме оригинал и перевод одновременно
        /// </summary>
        public virtual bool DiffTranslate => true;

        public void Activate(Resource res, Resource tres, bool translated)
        {
            _res = res;
            _tres = tres;
            _translated = translated;
            Reload();
        }

        protected virtual void Reload()
        {
        }

        public virtual void FocusRow(int value)
        {
        }

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

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }

        #endregion
    }
}
