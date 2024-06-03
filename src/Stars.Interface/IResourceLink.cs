namespace Terradue.Stars.Interface
{
    public interface IResourceLink : IResource
    {
        string Relationship { get; }

        string Title { get; }

    }
}
