// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: PluginLoadContext.cs

using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Terradue.Stars.Console
{
    internal class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyLoadContext mainAppAssemblyLoadContext;
        private readonly AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath, AssemblyLoadContext mainAppAssemblyLoadContext)
        {
            // try
            // {
            //     _resolver = new AssemblyDependencyResolver(pluginPath);
            // }
            // catch (InvalidOperationException e)
            // {
            //     string path = Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);
            //     PhysicalConsole.Singleton.Out.WriteLine(path);
            //     _resolver = new AssemblyDependencyResolver(Path.Combine(path, pluginPath));
            // }
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
