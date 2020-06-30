using System.Collections.Generic;
using Stars.Router;

namespace Stars.Router
{
    public interface IRoutable : IResource
    {
        

        IEnumerable<IRoute> GetRoutes();
    }
}