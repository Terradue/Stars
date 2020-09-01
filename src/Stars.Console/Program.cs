using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stars.Interface.Router;
using Stars.Interface.Router.Translator;
using Stars.Interface.Supply;
using Stars.Interface.Supply.Destination;
using Stars.Model.Atom;
using Stars.Model.Stac;
using Stars.Operations;
using Stars.Router;
using Stars.Supply;
using Stars.Supply.Destination;

namespace Stars
{
    class Program
    {
        private static IServiceProvider _serviceProvider;
        private static CommandLineApplication<Program> _app;

        public static IConfigurationRoot Configuration { get; set; }

        private static string[] _pluginAssemblies = new string[]
        {
            "Stars"
        };
        private static CommandOption _verboseOption;

        static int Main(string[] args)
        {
            _app = new CommandLineApplication<Program>();

            // init new CLI app
            ConfigureCommandLineApp();

            _app.OnParsingComplete(pr =>
            {
                // Register the DI services
                RegisterServices();
            }
            );

            int ret = _app.Execute(args);

            DisposeServices();

            return ret;
        }

        private static void LoadBase(ServiceCollection collection)
        {
            // Stac Router
            collection.AddTransient<IRouter, StacRouter>();
            // Atom Router
            collection.AddTransient<IRouter, AtomRouter>();

            // Generic Supplier
            collection.AddTransient<ISupplier, GenericSupplier>();

            // Local Filesystem destination
            collection.AddTransient<IDestinationGuide, LocalFileSystemDestinationGuide>();

            // Web Download Carrier
            collection.AddTransient<ICarrier, WebDownloadCarrier>();
        }


        private static void LoadPlugins(ServiceCollection collection)
        {



            // foreach (Type router in ResourceRoutersManager.LoadManagedPlugins(Configuration.GetSection("Routers")))
            // {
            //     collection.AddTransient(router);
            //     collection.AddTransient<IRouter>(serviceProvider => (IRouter)serviceProvider.GetService(router));
            // }

            foreach (ISupplier supplier in SupplierManager.LoadManagedPlugins(Configuration.GetSection("Suppliers"), _serviceProvider))
            {
                collection.AddSingleton<ISupplier>(supplier);
            }

            foreach(ITranslator translator in TranslatorManager.LoadManagedPlugins(Configuration.GetSection("Suppliers"), _serviceProvider))
            {
                collection.AddSingleton<ITranslator>(translator);
            }

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

        private static void ConfigureCommandLineApp()
        {

            _verboseOption = _app.Option("-v|--verbose", "Display operation details",
                                            CommandOptionType.NoValue);

            #region list-command
            _app.Command("list",
                (list) =>
                {
                    var outputOption = list.Option("-o|--output", "Output File",
                                                    CommandOptionType.SingleValue);

                    var inputOption = list.Option<string>("-i|--input", "Input reference to resource",
                                        CommandOptionType.MultipleValue)
                                        .IsRequired();

                    var recursivityOption = list.Option<int>("-r|--recursivity", "Resource recursivity depth routing",
                                        CommandOptionType.SingleValue);

                    var listAssetOption = list.Option("-sa|--skip-assets", "Do not list assets",
                                        CommandOptionType.NoValue);

                    list.Description = "List the Assets from the input reference";

                    list.OnExecuteAsync(async cancellationToken =>
                    {
                        var listOperation = _serviceProvider.GetService<ListOperation>();
                        await listOperation.ExecuteAsync();
                    });

                }
            );
            #endregion list-command

            #region copy-command
            _app.Command("copy",
                (copy) =>
                {
                    var outputOption = copy.Option<string>("-o|--output", "Output destination (folder, remote URI...)",
                                                    CommandOptionType.SingleValue)
                                                    .IsRequired();

                    var inputOption = copy.Option<string>("-i|--input", "Input reference to resource",
                                        CommandOptionType.MultipleValue)
                                        .IsRequired();

                    var recursivityOption = copy.Option<int>("-r|--recursivity", "Resource recursivity depth routing",
                                        CommandOptionType.SingleValue);

                    var skipAssetOption = copy.Option("-sa|--skip-assets", "Do not list assets",
                                        CommandOptionType.NoValue);

                    copy.Description = "Copy the Assets from the input reference to the output destination";


                    copy.OnExecuteAsync(async cancellationToken =>
                    {
                        var copyOperation = _serviceProvider.GetService<CopyOperation>();
                        await copyOperation.ExecuteAsync();
                    });

                }
            );
            #endregion copy-command



        }

        private static void RegisterServices()
        {
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
            collection.AddSingleton<CommandLineApplication>(_app);
            collection.AddSingleton<IConsole>(PhysicalConsole.Singleton);
            collection.AddSingleton<IReporter, StarsConsoleReporter>();
            collection.AddSingleton<ILogger, StarsConsoleReporter>();

            // Load Base Routers, Supplier, Destination and Carriers
            LoadBase(collection);

            _serviceProvider = collection.BuildServiceProvider();

            // Load Plugins
            LoadPlugins(collection);

            // Add the Managers
            collection.AddSingleton<ResourceRoutersManager, ResourceRoutersManager>();
            collection.AddSingleton<SupplierManager, SupplierManager>();
            collection.AddSingleton<DestinationManager, DestinationManager>();
            collection.AddSingleton<CarrierManager, CarrierManager>();

            // Add the operations
            collection.AddTransient<ListOperation, ListOperation>();
            collection.AddTransient<CopyOperation, CopyOperation>();

            // Build the service provider
            _serviceProvider = collection.BuildServiceProvider();

            _app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(_serviceProvider);

        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }

            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }

        }

        public static bool Verbose
        {
            get
            {
                return _verboseOption != null && _verboseOption.HasValue();
            }
        }
    }
}
