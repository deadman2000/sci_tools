
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts1_1;
using System;
using System.Linq;

namespace SCI_Lib.Resources
{
    public class ResScript : Resource
    {
        private IScript _script;

        public IScript GetScript()
        {
            if (_script != null) return _script;

            if (Package.SeparateHeapResources)
                return _script = new Script1_1(this);

            return _script = new Script(this);
        }

        public override string[] GetStrings()
        {
            return GetScript()?.AllStrings().Select(s => s.Value).ToArray();
        }

        public override void SetStrings(string[] strings)
        {
            var trScr = GetScript();

            var scriptStrings = trScr.AllStrings().ToArray();
            if (strings.Length != scriptStrings.Length)
                throw new Exception("Line count mismatch");

            for (int i = 0; i < strings.Length; i++)
            {
                if (strings[i] != null)
                    scriptStrings[i].Value = strings[i];
            }
        }

        public override byte[] GetPatch()
        {
            if (_script != null)
                return _script.GetBytes();
            return GetContent();
        }

        public void CleanCache()
        {
            _script = null;
        }
    }
}
