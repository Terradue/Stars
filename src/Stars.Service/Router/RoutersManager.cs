using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Stars.Interface.Router;

namespace Stars.Service.Router
{
    public class RoutersManager : AbstractManager<IRouter>
    {

        public RoutersManager(ILogger logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public IRouter GetRouter(INode resource)
        {
            return Plugins.FirstOrDefault(r => r.CanRoute(resource));
        }

    }
}