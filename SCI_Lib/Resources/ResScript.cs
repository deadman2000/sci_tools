
using SCI_Lib.Resources.Scripts;
using System;
using System.Linq;

namespace SCI_Lib.Resources
{
    public class ResScript : Resource
    {
        private Script _script;

        public Script GetScript()
        {
            if (Package.SeparateHeapResources)
                return null; // TODO Support external heap

            if (_script != null) return _script;
            return _script = new Script(this);
        }

        public override string[] GetStrings()
        {
            return GetScript()?.AllStrings.Select(s => s.Value).ToArray();
        }

        public override void SetStrings(string[] strings)
        {
            var trScr = GetScript();
            var scriptStrings = trScr.AllStrings.ToArray();

            if (strings.Length != scriptStrings.Length)
                throw new Exception("Line count mismatch");

            for (int i = 0; i < strings.Length; i++)
            {
                if (strings[i] != null)
                    scriptStrings[i].Value = strings[i];
            }

            SavePatch(trScr.GetBytes());
        }
    }
}
