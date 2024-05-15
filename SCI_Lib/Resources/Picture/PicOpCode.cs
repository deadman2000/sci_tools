namespace SCI_Lib.Resources.Picture
{
    public enum PicOpCode : byte
    {
        SET_COLOR = 0xf0,
        DISABLE_VISUAL = 0xf1,
        SET_PRIORITY = 0xf2,
        DISABLE_PRIORITY = 0xf3,
        RELATIVE_PATTERNS = 0xf4,
        RELATIVE_MEDIUM_LINES = 0xf5,
        RELATIVE_LONG_LINES = 0xf6,
        RELATIVE_SHORT_LINES = 0xf7,
        FILL = 0xf8,
        SET_PATTERN = 0xf9,
        ABSOLUTE_PATTERNS = 0xfa,
        SET_CONTROL = 0xfb,
        DISABLE_CONTROL = 0xfc,
        RELATIVE_MEDIUM_PATTERNS = 0xfd,
        OPX = 0xfe,
        END = 0xff
    }
}
