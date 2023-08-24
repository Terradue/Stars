// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: PluginManager.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Processing;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Plugins
{
    public class PluginManager
    {
        private readonly ILogger logger;
        protected readonly IServiceProvider serviceProvider;
        private readonly IOptions<PluginsOptions> options;

        public PluginManager(ILogger<PluginManager> logger, IServiceProvider serviceProvider, IOptions<PluginsOptions> options)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.options = options;
        }

        public void LoadPlugins(IServiceCollection collection, Func<string, AssemblyLoadContext> loadContextProvider = null)
        {
            logger.LogDebug("Loading Plugins");
            foreach (var pluginsOption in options.Value)
            {
                AssemblyLoadContext loadContext = AssemblyLoadContext.Default;
                Assembly assembly = Assembly.GetExecutingAssembly();

                if (pluginsOption.Value.Assembly != null && loadContextProvider != null)
                {
                    loadContext = loadContextProvider(pluginsOption.Value.Assembly);
                    assembly = loadContext.LoadFromAssemblyName(new AssemblyName(pluginsOption.Value.Assembly));
                }

                if (pluginsOption.Value.Routers != null)
                    RegisterConfiguredPlugins<IRouter>(pluginsOption.Value.Routers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as IPluginOption), collection, assembly);
                if (pluginsOption.Value.Suppliers != null)
                    RegisterConfiguredPlugins<ISupplier>(pluginsOption.Value.Suppliers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as IPluginOption), collection, assembly);
                if (pluginsOption.Value.Translators != null)
                    RegisterConfiguredPlugins<ITranslator>(pluginsOption.Value.Translators.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as IPluginOption), collection, assembly);
                if (pluginsOption.Value.Destinations != null)
                    RegisterConfiguredPlugins<IDestinationGuide>(pluginsOption.Value.Destinations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as IPluginOption), collection, assembly);
                if (pluginsOption.Value.Carriers != null)
                    RegisterConfiguredPlugins<ICarrier>(pluginsOption.Value.Carriers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as IPluginOption), collection, assembly);
                if (pluginsOption.Value.Processings != null)
                    RegisterConfiguredPlugins<IProcessing>(pluginsOption.Value.Processings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as IPluginOption), collection, assembly);
            }
        }

        public void RegisterConfiguredPlugins<T>(Dictionary<string, IPluginOption> plugins, IServiceCollection collection, Assembly assembly) where T : IPlugin
        {
            if (plugins == null) return;
            foreach (var pluginConfig in plugins)
            {
                if (string.IsNullOrEmpty(pluginConfig.Value.Type))
                    continue;
                try
                {
                    Type type = GetTypeFromAssembly(pluginConfig.Value.Type, assembly) ?? throw new DllNotFoundException(string.Format("Plugin {0} of type {1} not found.", pluginConfig.Key, pluginConfig.Value.Type));
                    collection.AddTransient(typeof(T), serviceProvider => CreateConfiguredPlugin<T>(serviceProvider, pluginConfig.Key, pluginConfig.Value, type));
                    logger.LogDebug("Plugin [{0}] injected", pluginConfig.Key);
                }
                catch (Exception e)
                {
                    logger.LogWarning("Impossible to load {0} : {1}", pluginConfig.Key, e.Message);
                }
            }
        }

        public static T CreateConfiguredPlugin<T>(IServiceProvider serviceProvider, string key, IPluginOption pluginOption, Type type) where T : IPlugin
        {
            int prio = 50;
            if (type.GetCustomAttribute(typeof(PluginPriorityAttribute)) is PluginPriorityAttribute prioAttr)
            {
                prio = prioAttr.Priority;
            }
            if (pluginOption.Priority.HasValue)
            {
                prio = pluginOption.Priority.Value;
            }
            return CreateItem<T>(type, pluginOption, prio, serviceProvider, key);
        }

        public static T CreateDefaultPlugin<T>(IServiceProvider serviceProvider, Type type) where T : IPlugin
        {
            int prio = 50;
            if (type.GetCustomAttribute(typeof(PluginPriorityAttribute)) is PluginPriorityAttribute prioAttr)
            {
                prio = prioAttr.Priority;
            }
            return CreateItem<T>(type, null, prio, serviceProvider, null);
        }

        private static T CreateItem<T>(Type type, IPluginOption pluginOption, int prio, IServiceProvider serviceProvider, string key) where T : IPlugin
        {
            if (!(typeof(T).IsAssignableFrom(type)))
                return default(T);

            object plugin = null;
            try
            {
                plugin = ActivatorUtilities.CreateInstance(serviceProvider, type, new object[] { pluginOption });
            }
            catch
            {
                plugin = ActivatorUtilities.CreateInstance(serviceProvider, type);
            }

            T item = (T)plugin;
            item.Priority = prio;
            if (!string.IsNullOrEmpty(key))
                item.Key = key;

            return item;
        }

        public static Type GetTypeFromAssembly(string typeName, Assembly assembly)
        {
            var type = assembly.GetType(typeName); ;
            if (type != null)
                return type;
            return null;
        }
    }
}
