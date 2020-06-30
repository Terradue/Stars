using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Stars.Router
{
    internal class ResourceRoutersManager
    {
        private IEnumerable<IResourceRouter> _routers;

        public ResourceRoutersManager(IEnumerable<IResourceRouter> routers)
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

        static Assembly LoadPlugin(string relativePath)
        {
            // Navigate to the bin dir
            string binDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location)));

            string pluginLocation = Path.GetFullPath(Path.Combine(binDir, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
            Console.WriteLine($"Loading routers from: {pluginLocation}");
            PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
        }

        internal static IEnumerable<Type> CreateResourceRouter(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetInterface(typeof(IResourceRouter).FullName) != null)
                {
                    yield return type;
                }
            }
        }

        internal IResourceRouter GetRouterForResource(IResource resource)
        {
            return _routers.FirstOrDefault(r => r.CanRoute(resource));
        }

    }
}