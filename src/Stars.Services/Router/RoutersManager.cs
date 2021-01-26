using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Services.Router
{
    public class RoutersManager : AbstractManager<IRouter>
    {

        public RoutersManager(ILogger<RoutersManager> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public IRouter GetRouter(IResource route)
        {
            return Plugins.Values.FirstOrDefault(r =>
            {
                try
                {
                    return r.CanRoute(route);
                }
                catch { return false; }
            });
        }

    }
}