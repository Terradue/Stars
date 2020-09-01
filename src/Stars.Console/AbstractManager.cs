using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace Stars
{
    public abstract class AbstractManager<T>
    {
        protected IEnumerable<T> _items;

        public AbstractManager(IEnumerable<T> items)
        {
            this._items = items;
        }

        public static IEnumerable<T> LoadManagedPlugins(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {
            IReporter reporter = (IReporter)serviceProvider.GetService(typeof(IReporter));
            SortedList<int, T> plugins = new SortedList<int, T>();
            foreach (var section in configurationSection.GetChildren())
            {
                if (string.IsNullOrEmpty(section["type"]))
                    continue;
                try
                {
                    Type type = GetType(section["type"]);
                    int prio = 50;
                    PluginPriorityAttribute prioAttr = type.GetCustomAttribute(typeof(PluginPriorityAttribute)) as PluginPriorityAttribute;
                    if (prioAttr != null)
                    {
                        prio = prioAttr.Priority;
                    }
                    if (!string.IsNullOrEmpty(section["priority"]))
                        prio = int.Parse(section["priority"]);
                    T item = CreateItem(type, section, serviceProvider);
                    if (item != null)
                        plugins.Add(prio, item);
                }
                catch (Exception e)
                {
                    reporter.Warn(string.Format("Impossible to load {0} : {1}", section.Key, e.Message));
                }
            }
            return plugins.Values;
        }

        public static T CreateItem(Type type, IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {

            if (type.GetInterface(typeof(T).FullName) == null)
                return default(T);

            MethodInfo createMethod = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);

            return (T)createMethod.Invoke(null, new object[2] { configurationSection, serviceProvider });


        }

        public static Type GetType(string typeName)
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