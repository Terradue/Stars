using System;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace Terradue.Stars
{
    internal class PluginLoadContext : AssemblyLoadContext
    {
        private readonly ILogger logger;
        private readonly AssemblyLoadContext mainAppAssemblyLoadContext;
        private AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath, ILogger logger, AssemblyLoadContext mainAppAssemblyLoadContext)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
            this.logger = logger;
            this.mainAppAssemblyLoadContext = mainAppAssemblyLoadContext;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            try
            {
                mainAppAssemblyLoadContext.LoadFromAssemblyName(assemblyName);
                logger.LogDebug("{0} already loaded", assemblyName);
                return null;
            }
            catch { }

            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
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