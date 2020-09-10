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

namespace Stars
{
    [Command(Name = "Stars", Description = "Spatio Temporal Asset Router & Supplier")]
    [HelpOption]
    [Subcommand(
        typeof(ListOperation)
        // typeof(CopyCommand)
    )]
    class Program
    {

        private readonly IStarsService _starService;

        public static IConfigurationRoot Configuration { get; set; }

        private static ILogger _logger;

        [Option]
        public bool Verbose { get; set; }

        static async Task<int> Main(string[] args)
        {
            CommandLineApplication<Program> app = new CommandLineApplication<Program>();

            var serviceProvider = RegisterServices(app);

            ConfigureCommandLineApp(app, serviceProvider);

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(serviceProvider);

            int ret = await app.ExecuteAsync(args);

            DisposeServices(serviceProvider);

            return ret;
        }

        public Program(IStarsService starService)
        {
            _starService = starService;
        }

        private async void OnExecuteAsync()
        {
            await _starService.InvokeAsync();
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

            // foreach (Type router in ResourceRoutersManager.LoadManagedPlugins(Configuration.GetSection("Routers")))
            // {
            //     collection.AddTransient(router);
            //     collection.AddTransient<IRouter>(serviceProvider => (IRouter)serviceProvider.GetService(router));
            // }

            SupplierManager.RegisterConfiguredPlugins(Configuration.GetSection("Suppliers"), collection, _logger);

            TranslatorManager.RegisterConfiguredPlugins(Configuration.GetSection("Translators"), collection, _logger);

            // foreach (Type destinationGuide in DestinationManager.LoadManagedPlugins(_pluginAssemblies))
            // {
            //     collection.AddTransient(destinationGuide);
            //     collection.AddTransient<IDestinationGuide>(serviceProvider => (IDestinationGuide)serviceProvider.GetService(destinationGuide));
            // }

            // foreach (Type carrierType in CarrierManager.LoadManagedPlugins(_pluginAssemblies))
            // {
            //     collection.AddTransient(carrierType);
            //     collection.AddTransient<ICarrier>(serviceProvider =>
            //     {
            //         var carrier = (ICarrier)serviceProvider.GetService(carrierType);
            //         carrier.Configure(Configuration.GetSection("Carriers").GetSection(carrier.Id));
            //         return carrier;
            //     });
            // }
        }

        private static void ConfigureCommandLineApp(CommandLineApplication app, IServiceProvider serviceProvider)
        {

            // #region copy-command
            // app.Command("copy",
            //     (copy) =>
            //     {
            //         var outputOption = copy.Option<string>("-o|--output", "Output destination (folder, remote URI...)",
            //                                         CommandOptionType.SingleValue)
            //                                         .IsRequired();

            //         var inputOption = copy.Option<string>("-i|--input", "Input reference to resource",
            //                             CommandOptionType.MultipleValue)
            //                             .IsRequired();

            //         var recursivityOption = copy.Option<int>("-r|--recursivity", "Resource recursivity depth routing",
            //                             CommandOptionType.SingleValue);

            //         var skipAssetOption = copy.Option("-sa|--skip-assets", "Do not list assets",
            //                             CommandOptionType.NoValue);

            //         copy.Description = "Copy the Assets from the input reference to the output destination";


            //         copy.OnExecuteAsync(async cancellationToken =>
            //         {
            //             var copyOperation = serviceProvider.GetService<CopyOperation>();
            //             await copyOperation.ExecuteAsync();
            //         });

            //     }
            // );
            // #endregion copy-command


            app.OnValidationError(ve =>
                {
                    PhysicalConsole.Singleton.Error.WriteLine(ve.ErrorMessage);
                    app.ShowHelp();
                    return 1;
                }
            );

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
            collection.AddSingleton<IStarsService, StarsCommandLineTool>();
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
