using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Stars
{
    public abstract class AbstractManager<T>
    {
        protected IEnumerable<T> _items;

        public AbstractManager(IEnumerable<T> items)
        {
            this._items = items;
        }

        internal static IEnumerable<Type> LoadManagedPlugins(string[] pluginPaths)
        {
            return pluginPaths.SelectMany(pluginPath =>
            {
                Assembly pluginAssembly = Assembly.Load(pluginPath);
                return CreateItems(pluginAssembly);
            });
        }

        internal static IEnumerable<Type> CreateItems(Assembly assembly)
        {
            SortedList<int, Type> types = new SortedList<int, Type>();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetInterface(typeof(T).FullName) != null)
                {
                    int prio = 50;
                    PluginPriorityAttribute prioAttr = type.GetCustomAttribute(typeof(PluginPriorityAttribute)) as PluginPriorityAttribute;
                    if (prioAttr != null)
                    {
                        prio = prioAttr.Priority;
                    }
                    types.Add(prio, type);
                }
            }
            return types.Values;
        }
    }
}