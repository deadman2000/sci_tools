using System;
using System.Collections.Generic;

namespace SCI_Lib.Pictures
{
    public class Sprite
    {
        public SpriteFrame this[int index]
        {
            get { return Frames[index]; }
            set { Frames[index] = value; }
        }

        public List<SpriteFrame> Frames { get; } = new List<SpriteFrame>();
    }
}
