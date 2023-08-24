// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: RoutersManager.cs

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
