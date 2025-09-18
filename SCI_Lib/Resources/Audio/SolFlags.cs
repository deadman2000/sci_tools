using System;

namespace SCI_Lib.Resources.Audio
{
    [Flags]
    public enum SolFlags
    {
        Compressed = 1,
        Unknown = 2,
        Is16Bit = 4,
        IsSigned = 8
    }
}
