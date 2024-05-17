using SCI_Lib.Resources.Scripts1;
using System;
using System.Linq;

namespace SCI_Lib.Resources
{
    public class ResHeap : Resource
    {
        private Heap _heap;
        
        public Heap GetHeap()
        {
            return _heap ??= new Heap(this);
        }

        public override string[] GetStrings()
        {
            return GetHeap()?.AllStrings().Select(s => s.Value).ToArray();
        }

        public override void SetStrings(string[] strings)
        {
            var heap = GetHeap();

            var scriptStrings = heap.AllStrings().ToArray();
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
            if (_heap != null)
                return _heap.GetBytes();
            return GetContent();
        }

        public void CleanCache()
        {
            _heap = null;
        }
    }
}
