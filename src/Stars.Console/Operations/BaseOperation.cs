using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Interface.Supply.Destination;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Catalog;
using Terradue.Stars.Services.Model.Atom;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Router.Translator;
using Terradue.Stars.Services.Supply;
using Terradue.Stars.Services.Supply.Carrier;
using Terradue.Stars.Services.Supply.Destination;
using Terradue.Stars.Services.Supply.Receipt;

namespace Terradue.Stars.Operations
{
    internal abstract class BaseOperation
    {
        protected static StarsConsoleReporter logger;

        protected IConsole console;

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

        private void LoadBase(ServiceCollection collection, IConfigurationRoot configuration)
        {
            // Stac Router
            collection.AddTransient<IRouter, StacRouter>();
            // Atom Router
            collection.AddTransient<IRouter, AtomRouter>();

            // Generic Supplier
            collection.AddTransient<ISupplier, NativeSupplier>();

            // Local Filesystem destination
            collection.AddTransient<IDestinationGuide, LocalFileSystemDestinationGuide>();

            // Carrier Options
            collection.Configure<GlobalOptions>(configuration.GetSection("Global"));
            // Web Download Carrier
            collection.AddTransient<ICarrier, LocalWebDownloadCarrier>();
            // Streaming Carrier
            collection.AddTransient<ICarrier, LocalStreamingCarrier>();

            // Routing Task
            collection.AddTransient<RoutingService, RoutingService>();
            // Supplying Task
            collection.AddTransient<SupplyService, SupplyService>();
            // Cataloging Task
            collection.AddTransient<CatalogingService, CatalogingService>();

            // Credentials Options & Manager
            collection.Configure<CredentialsOptions>(co => co.Configure(Configuration.GetSection("Credentials"), logger));
            collection.AddSingleton<ICredentials, ConsoleCredentialsManager>();
            collection.AddSingleton<ConsoleUserSettings, ConsoleUserSettings>();

        }


        private void LoadPlugins(ServiceCollection collection, IConfigurationRoot configuration)
        {
            RoutersManager.RegisterConfiguredPlugins(configuration.GetSection("Routers"), collection, logger);

            SupplierManager.RegisterConfiguredPlugins(configuration.GetSection("Suppliers"), collection, logger);

            TranslatorManager.RegisterConfiguredPlugins(configuration.GetSection("Translators"), collection, logger);

            DestinationManager.RegisterConfiguredPlugins(configuration.GetSection("Destinations"), collection, logger);

            CarrierManager.RegisterConfiguredPlugins(configuration.GetSection("Carriers"), collection, logger);

            ReceiptManager.RegisterConfiguredPlugins(configuration.GetSection("Receivers"), collection, logger);
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
            builder.AddNewtonsoftJsonFile(Path.Join(System.Environment.GetEnvironmentVariable("HOME"), ".config", "Stars", "usersettings.json"), optional: true, reloadOnChange: true)
                   .AddNewtonsoftJsonFile("appsettings.json", optional: true)
                   .AddNewtonsoftJsonFile("/etc/Stars/appsettings.json", optional: true, reloadOnChange: true);
            if (Directory.Exists("/etc/Stars/conf.d"))
            {
                foreach (var yamlFilename in Directory.EnumerateFiles("/etc/Stars/conf.d", "*.json", SearchOption.TopDirectoryOnly))
                    builder.AddNewtonsoftJsonFile(yamlFilename);
            }

            //only add secrets in development
            if (isDevelopment)
            {
                builder.AddUserSecrets<Program>();
            }

            Configuration = builder.Build();

            collection.AddSingleton<IConfigurationRoot>(Configuration);

            // Add the command line services
            collection.AddSingleton<IConsole>(PhysicalConsole.Singleton);
            collection.AddSingleton<ILogger>(logger);

            // Load Base Routers, Supplier, Destination and Carriers
            LoadBase(collection, Configuration);

            // Load Plugins
            LoadPlugins(collection, Configuration);

            // Add the Managers
            collection.AddSingleton<RoutersManager, RoutersManager>();
            collection.AddSingleton<SupplierManager, SupplierManager>();
            collection.AddSingleton<DestinationManager, DestinationManager>();
            collection.AddSingleton<CarrierManager, CarrierManager>();
            collection.AddSingleton<TranslatorManager, TranslatorManager>();
            collection.AddSingleton<ReceiptManager, ReceiptManager>();

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