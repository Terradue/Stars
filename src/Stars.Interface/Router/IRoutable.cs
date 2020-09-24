using System.Collections.Generic;

namespace Terradue.Stars.Interface.Router
{
    public interface IRoutable : INode
    {
        IList<IRoute> GetRoutes();
    }
}