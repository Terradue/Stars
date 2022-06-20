using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Services.Router
{
    public class RoutersManager : AbstractManager<IRouter>
    {

        public RoutersManager(ILogger<RoutersManager> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public async Task<IRouter> GetRouterAsync(IResource route)
        {
            // Force retrieaval of content-type from remote loacation
            // IResourceServiceProvider resourceServiceProvider = serviceProvider.GetService<IResourceServiceProvider>();
            // IStreamResource streamResource = await resourceServiceProvider.GetStreamResourceAsync(route);
            return GetPlugins().Values.FirstOrDefault(r =>
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