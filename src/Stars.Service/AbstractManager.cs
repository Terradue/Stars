using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Stars.Service
{
    public abstract class AbstractManager<T>
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;

        private static Dictionary<Type, int> pluginsPriority = new Dictionary<Type, int>();

        public AbstractManager(ILogger logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public static void RegisterConfiguredPlugins(IConfigurationSection configurationSection, ServiceCollection collection, ILogger logger)
        {

            foreach (var section in configurationSection.GetChildren())
            {
                if (string.IsNullOrEmpty(section["type"]))
                    continue;
                try
                {
                    Type type = GetTypeFromAssemblies(section["type"]);
                    if (type == null) throw new DllNotFoundException(string.Format("Plugin {0} for {1} not found.", section["type"], typeof(T)));
                    int prio = 50;
                    PluginPriorityAttribute prioAttr = type.GetCustomAttribute(typeof(PluginPriorityAttribute)) as PluginPriorityAttribute;
                    if (prioAttr != null)
                    {
                        prio = prioAttr.Priority;
                    }
                    if (!string.IsNullOrEmpty(section["priority"]))
                        prio = int.Parse(section["priority"]);
                    collection.AddTransient(typeof(T), serviceProvider => CreateItem(type, section, prio, serviceProvider));
                    if (pluginsPriority.ContainsKey(type))
                        pluginsPriority.Remove(type);
                    pluginsPriority.Add(type, prio);
                }
                catch (Exception e)
                {
                    logger.LogWarning("Impossible to load {0} : {1}", section.Key, e.Message);
                }
            }
        }

        public List<T> Plugins
        {
            get
            {
                List<(int, T)> sortedList = new List<(int, T)>();
                foreach (var plugin in serviceProvider.GetServices<T>())
                {
                    int prio = pluginsPriority.ContainsKey(plugin.GetType()) ? pluginsPriority[plugin.GetType()] : 50;
                    sortedList.Add((prio, plugin));
                }
                sortedList.Sort((a, b) => (a.Item1.CompareTo(b.Item1)));
                return sortedList.Select(i => i.Item2).ToList();
            }
        }

        private static T CreateItem(Type type, IConfigurationSection configurationSection, int prio, IServiceProvider serviceProvider)
        {

            if (type.GetInterface(typeof(T).FullName) == null)
                return default(T);

            MethodInfo createMethod = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);

            return (T)createMethod.Invoke(null, new object[2] { configurationSection, serviceProvider });
        }

        public static Type GetTypeFromAssemblies(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }
    }
}