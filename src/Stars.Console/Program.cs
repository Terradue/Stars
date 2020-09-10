using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stars.Interface.Router;
using Stars.Interface.Router.Translator;
using Stars.Interface.Supply;
using Stars.Interface.Supply.Destination;
using Stars.Service.Model.Atom;
using Stars.Service.Model.Stac;
using Stars.Operations;
using Stars.Service.Router;
using Stars.Service.Supply;
using Stars.Service.Supply.Destination;
using Stars.Service.Router.Translator;
using Stars.Interface;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils.Conventions;

namespace Stars
{
    [Command(Name = "Stars", Description = "Spatio Temporal Asset Router & Supplier")]
    [HelpOption]
    [Subcommand(
        typeof(ListOperation)
    // typeof(CopyCommand)
    )]
    public class Program
    {

        public static IConfigurationRoot Configuration { get; set; }

        private static ILogger _logger;

        [Option]
        public bool Verbose { get; set; }

        static async Task<int> Main(string[] args)
        {
            CommandLineApplication<Program> app = new CommandLineApplication<Program>();

            var serviceProvider = RegisterServices(app);

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(serviceProvider);

            try
            {
                return await app.ExecuteAsync(args);
            }
            catch (CommandParsingException cpe)
            {
                return PrintErrorAndUsage(cpe.Command, cpe.Message);
            }
            finally
            {
                DisposeServices(serviceProvider);
            }


        }

        private static int PrintErrorAndUsage(CommandLineApplication command, string message)
        {
            PhysicalConsole.Singleton.Error.WriteLineAsync("Parsing Error: " + message);
            command.ShowHelp();
            return 2;
        }

        private async void OnExecuteAsync(CommandLineApplication app)
        {
            await PhysicalConsole.Singleton.Error.WriteLineAsync("Specify a subcommand");
            app.ShowHelp();
        }

        private static void LoadBase(ServiceCollection collection)
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
        }


        private static void LoadPlugins(ServiceCollection collection)
        {
            RoutersManager.RegisterConfiguredPlugins(Configuration.GetSection("Routers"), collection, _logger);

            SupplierManager.RegisterConfiguredPlugins(Configuration.GetSection("Suppliers"), collection, _logger);

            TranslatorManager.RegisterConfiguredPlugins(Configuration.GetSection("Translators"), collection, _logger);

            DestinationManager.RegisterConfiguredPlugins(Configuration.GetSection("Destinations"), collection, _logger);

            CarrierManager.RegisterConfiguredPlugins(Configuration.GetSection("Carriers"), collection, _logger);
        }


        public static int OnValidationError(CommandLineApplication command, ValidationResult ve)
        {
            PhysicalConsole.Singleton.Error.WriteLine(ve.ErrorMessage);
            command.ShowHelp();
            return 1;
        }

        private static IServiceProvider RegisterServices(CommandLineApplication app)
        {

            _logger = new StarsConsoleReporter(PhysicalConsole.Singleton);

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
            collection.AddSingleton<CommandLineApplication>(app);
            collection.AddSingleton<IConsole>(PhysicalConsole.Singleton);
            collection.AddSingleton<ILogger>(_logger);

            // Load Base Routers, Supplier, Destination and Carriers
            LoadBase(collection);

            // Load Plugins
            LoadPlugins(collection);

            // Add the Managers
            collection.AddSingleton<RoutersManager, RoutersManager>();
            collection.AddSingleton<SupplierManager, SupplierManager>();
            collection.AddSingleton<DestinationManager, DestinationManager>();
            collection.AddSingleton<CarrierManager, CarrierManager>();
            collection.AddSingleton<TranslatorManager, TranslatorManager>();

            // Add the operations
            collection.AddTransient<ListOperation, ListOperation>();
            // collection.AddTransient<CopyOperation, CopyOperation>();

            // Build the service provider
            var serviceProvider = collection.BuildServiceProvider();

            return serviceProvider;

        }

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
