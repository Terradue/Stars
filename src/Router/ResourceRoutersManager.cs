using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Stars.Router
{
    public class ResourceRoutersManager
    {
        private IEnumerable<IRouter> _routers;

        public ResourceRoutersManager(IEnumerable<IRouter> routers)
        {
            this._routers = routers;
        }

        internal static IEnumerable<Type> LoadRoutersPlugins(string[] pluginPaths)
        {
            return pluginPaths.SelectMany(pluginPath =>
            {
                Assembly pluginAssembly = Assembly.Load(pluginPath);
                return CreateResourceRouter(pluginAssembly);
            });
        }

        internal static IEnumerable<Type> CreateResourceRouter(Assembly assembly)
        {
            SortedList<int, Type> types = new SortedList<int, Type>();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetInterface(typeof(IRouter).FullName) != null)
                {
                    int prio = 50;
                    RouterPriorityAttribute prioAttr = type.GetCustomAttribute(typeof(RouterPriorityAttribute)) as RouterPriorityAttribute ;
                    if ( prioAttr != null ){
                        prio = prioAttr.Priority;
                    }
                    types.Add(prio, type);
                }
            }
            return types.Values;
        }

        internal IRouter GetRouter(IResource resource)
        {
            return _routers.FirstOrDefault(r => r.CanRoute(resource));
        }
    }
}