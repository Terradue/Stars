using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Interface.Supply.Destination;
using Stars.Service.Catalog;
using Stars.Service.Model.Atom;
using Stars.Service.Model.Stac;
using Stars.Service.Router;
using Stars.Service.Router.Translator;
using Stars.Service.Supply;
using Stars.Service.Supply.Destination;

namespace Stars.Operations
{
    internal abstract class BaseOperation
    {
        protected static StarsConsoleReporter logger;

        private IConsole console;

        public BaseOperation()
        {
            console = PhysicalConsole.Singleton;
        }

        protected IServiceProvider ServiceProvider { get; private set; }
        public static IConfigurationRoot Configuration { get; private set; }

        public ValidationResult OnValidate()
        {
            var serviceCollection = RegisterServices();
            // Build the service provider
            ServiceProvider = serviceCollection.BuildServiceProvider();

            return ValidationResult.Success;
        }

        public async Task<int> OnExecuteAsync()
        {
            try
            {
                await ExecuteAsync();
            }
            catch (CommandParsingException cpe)
            {
                return Program.PrintErrorAndUsage(cpe.Command, cpe.Message);
            }
            finally
            {
                DisposeServices(ServiceProvider);
            }
            return 0;
        }

        protected abstract Task ExecuteAsync();

        private void LoadBase(ServiceCollection collection)
        {
            // Stac Router
            collection.AddTransient<IRouter, StacRouter>();
            // Atom Router
            collection.AddTransient<IRouter, AtomRouter>();

            // Generic Supplier
            collection.AddTransient<ISupplier, NativeSupplier>();

            // Local Filesystem destination
            collection.AddTransient<IDestinationGuide, LocalFileSystemDestinationGuide>();

            // Web Download Carrier
            collection.AddTransient<ICarrier, WebDownloadCarrier>();
            // Streaming Carrier
            collection.AddTransient<ICarrier, StreamingCarrier>();

            // Routing Task
            collection.AddTransient<RoutingTask, RoutingTask>();
            // Supplying Task
            collection.AddTransient<SupplyingTask, SupplyingTask>();
            // Cataloging Task
            collection.AddTransient<CatalogingTask, CatalogingTask>();
        }


        private void LoadPlugins(ServiceCollection collection, IConfigurationRoot configuration)
        {
            RoutersManager.RegisterConfiguredPlugins(configuration.GetSection("Routers"), collection, logger);

            SupplierManager.RegisterConfiguredPlugins(configuration.GetSection("Suppliers"), collection, logger);

            TranslatorManager.RegisterConfiguredPlugins(configuration.GetSection("Translators"), collection, logger);

            DestinationManager.RegisterConfiguredPlugins(configuration.GetSection("Destinations"), collection, logger);

            CarrierManager.RegisterConfiguredPlugins(configuration.GetSection("Carriers"), collection, logger);
        }

        private ServiceCollection RegisterServices()
        {

            logger = new StarsConsoleReporter(PhysicalConsole.Singleton, Program.Verbose);

            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            //Determines the working environment as IHostingEnvironment is unavailable in a console app
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                devEnvironmentVariable.ToLower() == "development";

            var collection = new ServiceCollection();

            // Add Configuration
            var builder = new ConfigurationBuilder();
            // tell the builder to look for the appsettings.json file
            builder.AddYamlFile("appsettings.yml", optional: true)
                    .AddNewtonsoftJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            //only add secrets in development
            if (isDevelopment)
            {
                builder.AddUserSecrets<Program>();
            }

            Configuration = builder.Build();

            collection.AddSingleton<IConfiguration>(Configuration);

            // Add the command line services
            collection.AddSingleton<IConsole>(PhysicalConsole.Singleton);
            collection.AddSingleton<ILogger>(logger);

            // Load Base Routers, Supplier, Destination and Carriers
            LoadBase(collection);

            // Load Plugins
            LoadPlugins(collection, Configuration);

            // Add the Managers
            collection.AddSingleton<RoutersManager, RoutersManager>();
            collection.AddSingleton<SupplierManager, SupplierManager>();
            collection.AddSingleton<DestinationManager, DestinationManager>();
            collection.AddSingleton<CarrierManager, CarrierManager>();
            collection.AddSingleton<TranslatorManager, TranslatorManager>();

            // Registers services specific to operation
            RegisterOperationServices(collection);


            return collection;

        }

        protected abstract void RegisterOperationServices(ServiceCollection collection);

        private static void DisposeServices(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                return;
            }

            if (serviceProvider is IDisposable)
            {
                ((IDisposable)serviceProvider).Dispose();
            }

        }

    }
}