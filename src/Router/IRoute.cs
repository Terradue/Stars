using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Stars.Router
{
    public interface IRoute
    {
        Uri Uri { get; }

        ContentType ContentType { get; }
        bool CanGetResource { get; }
        ResourceType ResourceType { get; }

        Task<IResource> GetResource();
    }
}