using System.Collections.Generic;

namespace SCI_Lib.Resources.View
{
    public class Loop
    {
        public List<Cell> Cells { get; } = new List<Cell>();
        public bool IsMirror { get; internal set; }
        public byte LoopMirror { get; internal set; }
    }
}
