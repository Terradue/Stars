using System;
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

        /// <summary>
        /// Return a new plugin list
        /// </summary>
        /// <returns></returns>
        public PluginList<T> GetPlugins()
        {
            return new PluginList<T>(serviceProvider.GetServices<T>());
        }
    }
}
