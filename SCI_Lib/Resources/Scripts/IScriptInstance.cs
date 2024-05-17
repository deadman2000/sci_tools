namespace SCI_Lib.Resources.Scripts
{
    public interface IScriptInstance
    {
        ushort GetProperty(string name);

        void SetProperty(string name, ushort value);
    }
}