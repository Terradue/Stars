using System.Collections.Generic;

namespace Stars.Interface.Router
{
    public interface IRoutable : IResource
    {
        IEnumerable<IRoute> GetRoutes();
    }
}