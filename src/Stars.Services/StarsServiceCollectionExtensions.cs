using System;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Supplier.Destination;
using Terradue.Stars.Services.Translator;
using Terradue.Stars.Services.Processing;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Services.Credentials;
using System.Linq;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Model.Atom;
using System.Runtime.Loader;
using Terradue.Stars.Services.Plugins;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface.Router.Translator;
using System.IO.Abstractions;
using Terradue.Stars.Services.Resources;
using Terradue.Stars.Interface;
using System.Net.Http;
using Microsoft.Extensions.Caching.InMemory;
using Microsoft.Extensions.Caching.Abstractions;

namespace Terradue.Stars.Services
{
    public static class StarsServiceCollectionExtensions
    {

        public static IServiceCollection AddStarsManagedServices(this IServiceCollection services, Action<IStarsBuilder> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            // Add default credentials manager
            services.AddSingleton<ICredentials, ConfigurationCredentialsManager>();

            services.AddSingleton<S3ClientFactory>();
            services.AddHttpClient();
            services.AddHttpClient<HttpClient>("stars").ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var httpClientHandler = new HttpClientHandler()
                {
                    UseDefaultCredentials = true,
                    Credentials = sp.GetRequiredService<ICredentials>()
                };
                var cacheExpirationPerHttpResponseCode = CacheExpirationProvider.CreateSimple(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
                return new InMemoryCacheHandler(httpClientHandler, cacheExpirationPerHttpResponseCode);
            });

            //  ## Add plugins
            //  1. Add predefined plugins
            LoadBasePlugins(services);

            // 2. Add the Managers
            services.AddSingleton<RoutersManager, RoutersManager>();
            services.AddSingleton<SupplierManager, SupplierManager>();
            services.AddSingleton<DestinationManager, DestinationManager>();
            services.AddSingleton<CarrierManager, CarrierManager>();
            services.AddSingleton<TranslatorManager, TranslatorManager>();
            services.AddSingleton<ProcessingManager, ProcessingManager>();

            // 3. Let's Configure
            var builder = new StarsBuilder(services);
            configure(builder);

            return services;
        }

        public static IServiceCollection LoadConfiguredStarsPlugin(this IServiceCollection services, Func<string, AssemblyLoadContext> loadContextProvider)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            PluginManager pluginManager = new PluginManager(
                serviceProvider.GetService<ILogger<PluginManager>>(),
                serviceProvider,
                serviceProvider.GetService<IOptions<PluginsOptions>>()
            );
            pluginManager.LoadPlugins(services, loadContextProvider);
            return services;
        }

        public static IServiceCollection LoadBasePlugins(this IServiceCollection services)
        {
            services.AddTransient<IRouter, StacRouter>();
            // Atom Router
            services.AddTransient<IRouter, AtomRouter>();

            // Generic Supplier
            services.AddTransient<ISupplier, NativeSupplier>();

            // Local Filesystem destination
            services.AddTransient<IDestinationGuide, LocalFileSystemDestinationGuide>();
            services.AddSingleton<IFileSystem, FileSystem>();

            // S3 destination
            services.AddTransient<IDestinationGuide, S3DestinationGuide>();

            // Local Streaming Carrier
            services.AddTransient<ICarrier, LocalStreamingCarrier>();

            // S3 Streaming Carrier
            services.AddTransient<ICarrier, S3StreamingCarrier>();

            // Routing Service
            services.AddTransient<RouterService, RouterService>();
            // Supplying Service
            services.AddTransient<AssetService, AssetService>();
            // Processing Service
            services.AddTransient<ProcessingService, ProcessingService>();

            services.AddTransient<ITranslator>(serviceProvider => PluginManager.CreateDefaultPlugin<ITranslator>(serviceProvider, typeof(StacLinkTranslator)));
            services.AddTransient<ITranslator>(serviceProvider => PluginManager.CreateDefaultPlugin<ITranslator>(serviceProvider, typeof(DefaultStacTranslator)));

            services.AddSingleton<IResourceServiceProvider, DefaultResourceServiceProvider>();

            return services;

        }

        public static IServiceCollection UseCredentialsManager<T>(this IServiceCollection services) where T : class, ICredentials
        {
            services.RemoveAll<ICredentials>();
            services.AddSingleton<ICredentials, T>();
            return services;
        }


        public static IServiceCollection RemoveAll<T>(this IServiceCollection services)
        {
            foreach (var serviceDescriptor in services.Where(descriptor => descriptor.ServiceType == typeof(T)).ToArray())
                services.Remove(serviceDescriptor);

            return services;
        }
    }
}