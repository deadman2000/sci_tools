namespace SCI_Lib.Resources.Scripts
{
    public interface IClass
    {
        ushort Id { get; }
        
        string Name { get; }


        ushort GetProperty(string name);
        
        string GetPropertyName(int index);

        void SetProperty(string name, ushort value);

        bool HasProperty(string name);

        bool HasProperty(ushort selector);
    }
}