using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Stars.Router
{
    public interface IRoute
    {
        Uri Uri { get; }
        ContentType ContentType { get; }
        ResourceType ResourceType { get; }
        Task<IResource> GotoResource();
    }
}