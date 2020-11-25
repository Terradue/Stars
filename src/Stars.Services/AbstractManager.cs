using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Services
{
    public abstract class AbstractManager<T> where T : class, IPlugin
    {
        private readonly ILogger logger;
        protected readonly IServiceProvider serviceProvider;

        public AbstractManager(ILogger logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }


        public PluginList<T> Plugins
        {
            get
            {
                return new PluginList<T>(serviceProvider.GetServices<T>());
            }
        }
    }
}