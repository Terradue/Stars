using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stars.Router
{
    public interface IResourceRouter
    {
        bool CanRoute(IResource resource);

        IRoutable Route(IResource resource);
    }
}