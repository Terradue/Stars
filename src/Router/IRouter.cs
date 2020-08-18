using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stars.Router
{
    public interface IRouter
    {
        string Label { get; }

        bool CanRoute(IResource resource);

        Task<IRoutable> Go(IResource resource);
    }
}