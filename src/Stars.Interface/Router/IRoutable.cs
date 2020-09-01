using System.Collections.Generic;

namespace Stars.Interface.Router
{
    public interface IRoutable : INode
    {
        IEnumerable<IRoute> GetRoutes();
    }
}