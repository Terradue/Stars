using System;
using System.Net;
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
using System.Net.S3;
using Amazon.Extensions.NETCore.Setup;

namespace Terradue.Stars.Services
{
    public static class StarsServiceCollectionExtensions
    {

        public static IServiceCollection AddStarsManagedServices(this IServiceCollection services, Action<IServiceProvider, StarsConfiguration> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            // Add Credentials from config
            services.AddOptions<CredentialsOptions>().Configure<StarsConfiguration>((co, sc) => co.Load(sc.CredentialsOptions));
            // Add default credentials manager
            services.AddSingleton<ICredentials, ConfigurationCredentialsManager>();
            services.AddSingleton<AWSOptions>(sp => sp.GetService<StarsConfiguration>().AWSOptions);

            //  ## Add plugins
            //  1. Add predefined plugins
            LoadBasePlugins(services);
            // Add Plugins from config
            services.AddOptions<PluginsOptions>().Configure<StarsConfiguration>((co, sc) => co.Load(sc.PluginsOptions));

            // 2. Add the Managers
            services.AddSingleton<RoutersManager, RoutersManager>();
            services.AddSingleton<SupplierManager, SupplierManager>();
            services.AddSingleton<DestinationManager, DestinationManager>();
            services.AddSingleton<CarrierManager, CarrierManager>();
            services.AddSingleton<TranslatorManager, TranslatorManager>();
            services.AddSingleton<ProcessingManager, ProcessingManager>();

            // Finally, let's configure
            services.AddSingleton<StarsConfiguration>(serviceProvider =>
            {
                var configurationInstance = StarsConfiguration.Configuration;

                // init defaults for log provider and job activator
                // they may be overwritten by the configuration callback later

                // var scopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
                // if (scopeFactory != null)
                // {
                //     configurationInstance.UseActivator(new AspNetCoreJobActivator(scopeFactory));
                // }

                // do configuration inside callback
                configure(serviceProvider, configurationInstance);

                WebRequest.RegisterPrefix("s3",
                    new S3WebRequestCreate(serviceProvider.GetService<ILogger<S3WebRequest>>(),
                                            configurationInstance.AWSOptions,
                                            configurationInstance.S3BucketsOptions
                                            ));
                if (!UriParser.IsKnownScheme("s3"))
                    UriParser.Register(new S3UriParser(), "s3", -1);

                return configurationInstance;
            });

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
            services.AddTransient<ITranslator, StacLinkTranslator>();
            services.AddTransient<ITranslator, DefaultStacTranslator>();

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