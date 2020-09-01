using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Stars.Interface.Router;

namespace Stars.Router
{
    public class ResourceRoutersManager : AbstractManager<IRouter>
    {

        public ResourceRoutersManager(IEnumerable<IRouter> routers) : base(routers)
        {
        }

        internal IRouter GetRouter(INode resource)
        {
            return _items.FirstOrDefault(r => r.CanRoute(resource));
        }
    }
}