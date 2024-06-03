namespace Terradue.Stars.Interface
{
    public interface IPlugin
    {
        int Priority { get; set; }
        string Key { get; set; }

    }
}
