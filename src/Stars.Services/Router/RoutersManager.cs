using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Services.Router
{
    public class RoutersManager : AbstractManager<IRouter>
    {

        public RoutersManager(ILogger logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public IRouter GetRouter(IRoute route)
        {
            return Plugins.FirstOrDefault(r => r.CanRoute(route));
        }

    }
}