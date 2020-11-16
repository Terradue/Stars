using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;

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

        public static void RegisterConfiguredPlugins(IConfigurationSection configurationSection, ServiceCollection collection, ILogger logger, Assembly assembly)
        {
            foreach (var section in configurationSection.GetChildren())
            {
                if (string.IsNullOrEmpty(section["Type"]))
                    continue;
                try
                {
                    Type type = GetTypeFromAssembly(section["Type"], assembly);
                    if (type == null) throw new DllNotFoundException(string.Format("Plugin {0} for {1} not found.", section["type"], typeof(T)));
                    int prio = 50;
                    PluginPriorityAttribute prioAttr = type.GetCustomAttribute(typeof(PluginPriorityAttribute)) as PluginPriorityAttribute;
                    if (prioAttr != null)
                    {
                        prio = prioAttr.Priority;
                    }
                    if (!string.IsNullOrEmpty(section["Priority"]))
                        prio = int.Parse(section["Priority"]);
                    collection.AddTransient(typeof(T), serviceProvider => CreateItem(type, section, prio, serviceProvider));
                    logger.LogDebug("Plugin [{0}] injected", section.Key);
                }
                catch (Exception e)
                {
                    logger.LogWarning("Impossible to load {0} : {1}", section.Key, e.Message);
                }
            }
        }

        public PluginList<T> Plugins
        {
            get
            {
                return new PluginList<T>(serviceProvider.GetServices<T>());
            }
        }

        private static T CreateItem(Type type, IConfigurationSection configurationSection, int prio, IServiceProvider serviceProvider)
        {

            if (!(typeof(T).IsAssignableFrom(type)))
                return default(T);

            var plugin = Activator.CreateInstance(type);
            T item = plugin as T;
            item.Priority = prio;
            item.Key = string.IsNullOrEmpty(configurationSection.Key) ? Guid.NewGuid().ToString() : configurationSection.Key;
            item.Configure(configurationSection, serviceProvider);

            return item;

        }

        public static Type GetTypeFromAssembly(string typeName, Assembly assembly)
        {
            var type = assembly.GetType(typeName);
            if (type != null)
                return type;
            return null;
        }
    }
}