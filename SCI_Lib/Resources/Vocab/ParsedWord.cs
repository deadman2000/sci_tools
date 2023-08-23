namespace SCI_Lib.Resources.Vocab;

public struct ParsedWord
{
    public WordClass Class;
    public ushort Group;

    public override readonly string ToString() => $"[{Group:X3} {Class}]";
}
