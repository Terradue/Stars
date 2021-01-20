using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Loader;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Services;

namespace Terradue.Stars.Console.Operations
{
    internal abstract class BaseOperation
    {
        [Option]
        public static bool Verbose { get; set; }

        [Option("-conf|--config-file", "Config file to use", CommandOptionType.SingleOrNoValue)]
        public string ConfigFile { get; set; }

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


        private ServiceCollection RegisterServices()
        {

            logger = new StarsConsoleReporter(PhysicalConsole.Singleton, Verbose);

            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            //Determines the working environment as IHostingEnvironment is unavailable in a console app
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                devEnvironmentVariable.ToLower() == "development";

            var collection = new ServiceCollection();

            // Add logging
            collection.AddLogging(c => c.AddProvider(new StarsConsoleLoggerProvider(PhysicalConsole.Singleton, Verbose))
                        .AddFilter(logLevel => logLevel > LogLevel.Debug || Verbose));

            // Add Configuration
            var builder = new ConfigurationBuilder();
            // tell the builder to look for the appsettings.json file
            builder.AddNewtonsoftJsonFile("/etc/Stars/appsettings.json", optional: true, reloadOnChange: true);
            if (Directory.Exists("/etc/Stars/conf.d"))
            {
                foreach (var yamlFilename in Directory.EnumerateFiles("/etc/Stars/conf.d", "*.json", SearchOption.TopDirectoryOnly))
                    builder.AddNewtonsoftJsonFile(yamlFilename);
            }
            builder.AddNewtonsoftJsonFile(Path.Join(System.Environment.GetEnvironmentVariable("HOME"), ".config", "Stars", "usersettings.json"), optional: true, reloadOnChange: true)
                   .AddNewtonsoftJsonFile("appsettings.json", optional: true);

            //only add secrets in development
            if (isDevelopment)
            {
                builder.AddNewtonsoftJsonFile("appsettings.Development.json", optional: true);
                builder.AddUserSecrets<Program>();
            }

            if (!string.IsNullOrEmpty(ConfigFile))
            {
                if (!File.Exists(ConfigFile))
                    throw new FileNotFoundException(ConfigFile);
                builder.AddNewtonsoftJsonFile(ConfigFile, optional: true);
            }

            Configuration = builder.Build();

            collection.AddSingleton<IConfigurationRoot>(Configuration);

            // Add the command line services
            collection.AddSingleton<IConsole>(PhysicalConsole.Singleton);
            collection.AddSingleton<ILogger>(logger);

            // Add Stars Services
            collection.AddStarsServices((provider, configuration) => {
                configuration
                    .UseGlobalConfiguration(Configuration);

            });
            collection.LoadConfiguredStarsPlugin((assemblyPath) =>
            {
                if (!File.Exists(assemblyPath))
                {
                    logger.LogWarning("No assembly file at {0}", assemblyPath);
                }
                logger.LogDebug("Loading plugins from {0}", assemblyPath);
                return new PluginLoadContext(assemblyPath, AssemblyLoadContext.Default);
            });
            // Use also the console for the credentials
            collection.AddSingleton<ConsoleUserSettings, ConsoleUserSettings>();
            collection.UseCredentialsManager<ConsoleCredentialsManager>();

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