using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Terradue.Stars.Console
{
    internal class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyLoadContext mainAppAssemblyLoadContext;
        private AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath, AssemblyLoadContext mainAppAssemblyLoadContext)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
            this.mainAppAssemblyLoadContext = mainAppAssemblyLoadContext;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            try
            {
                var assembly = mainAppAssemblyLoadContext.LoadFromAssemblyName(assemblyName);
                string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
                Assembly pluginAssembly = null;
                if (assemblyPath != null)
                {
                    pluginAssembly = LoadFromAssemblyPath(assemblyPath);
                }
                return null;
            }
            catch (Exception)
            {
                string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
                if (assemblyPath != null)
                {
                    return LoadFromAssemblyPath(assemblyPath);
                }
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}