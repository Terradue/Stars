using System.Collections.Generic;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface.Router
{
    public interface IRouter : IPlugin
    {
        string Label { get; }

        bool CanRoute(IResource node);

        Task<IResource> Route(IResource node);

        Task<IResource> RouteLink(IResource resource, IResourceLink childLink);
    }
}