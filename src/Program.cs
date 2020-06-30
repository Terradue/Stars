using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Stars.Operations;
using Stars.Router;

namespace Stars
{
    class Program
    {
        private static IServiceProvider _serviceProvider;
        private static CommandLineApplication<Program> _app;

        private static string[] _pluginAssemblies = new string[]
        {
            "Stars"
        };
        private static CommandOption _verboseOption;

        static int Main(string[] args)
        {
            _app = new CommandLineApplication<Program>();

            // Register the DI services
            RegisterServices();

            // init new CLI app
            ConfigureCommandLineApp();

            int ret = _app.Execute(args);

            DisposeServices();

            return ret;
        }

        private static void AddPlugins(ServiceCollection collection)
        {
            foreach (Type router in ResourceRoutersManager.LoadRoutersPlugins(_pluginAssemblies))
            {
                collection.AddTransient(router);
                collection.AddTransient<IResourceRouter>(serviceProvider => (IResourceRouter)serviceProvider.GetService(router));
            }
        }

        private static void ConfigureCommandLineApp()
        {
            // var inputFileOption = _app.Option<string>("-i|--input", "Input reference to resource",
            //                                             CommandOptionType.MultipleValue)
            //                                             .IsRequired();

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

                    list.Description = "List the Assets from the input reference";

                    list.HelpOption("-? | -h | --help");

                    list.OnExecuteAsync(async cancellationToken =>
                    {
                        var listOperation = _serviceProvider.GetService<ListOperation>();
                        await listOperation.ExecuteAsync(inputOption.Values, recursivityOption.HasValue() ? recursivityOption.ParsedValue : 0);
                    });

                }
            );
            #endregion list-command

            // rest of CLI implementions
            _app.HelpOption("-h|--help|-?");

        }

        private static void RegisterServices()
        {
            var collection = new ServiceCollection();

            // Load Plugins
            AddPlugins(collection);

            // Add the command line services
            collection.AddSingleton<CommandLineApplication>(_app);
            collection.AddSingleton<IConsole>(PhysicalConsole.Singleton);
            collection.AddSingleton<IReporter, StacConsoleReporter>();
            // Add the Resource grabber
            collection.AddSingleton<ResourceGrabber, ResourceGrabber>();
            // Add the Routers Manager
            collection.AddSingleton<ResourceRoutersManager, ResourceRoutersManager>();

            // Add the operations
            collection.AddTransient<ListOperation, ListOperation>();

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
