using System.Net.Mime;

namespace Terradue.Stars.Interface
{
    public interface IResource : ILocatable
    {
        ContentType ContentType { get; }
        ResourceType ResourceType { get; }
        ulong ContentLength { get; }
        ContentDisposition ContentDisposition { get; }
    }
}
