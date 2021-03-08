namespace SCI_Lib.Resources
{
    public enum ResType : byte
    {
        View = 0x80,
        Picture = 0x81,
        Script = 0x82,
        Text = 0x83,
        Sound = 0x84,
        Vocabulary = 0x86,
        Font = 0x87,
        Cursor = 0x88,
        AudioPath = 0x89,
        Bitmap = 0x8a,
        Palette = 0x8b,
        CDAudio = 0x8c,
        Audio = 0x8d,
        Sync = 0x8e,
        Message = 0x8f,
        Map = 0x90,
        Heap = 0x91,

        End = 0xFF,


        Memory = 0x0,
        Patch = 0x1,
        Audio36 = 0x2,
        Sync36 = 0x3,
        Translation = 0x4,
        Rave = 0x5
    }
}
