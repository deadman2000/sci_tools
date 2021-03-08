using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCI_Lib.Resources.View
{
    public class Cell
    {
        public ushort Width { get; set; }

        public ushort Height { get; set; }

        public byte TransparentColor { get; set; }
        
        public byte PlacementX { get; set; }
        
        public byte PlacementY { get; set; }

        public byte[] Pixels { get; set; }
    }
}
