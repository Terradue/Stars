using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Runtime.Loader;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Services;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Terradue.Stars.Console.Operations
{
    internal abstract class BaseOperation
    {
        [Option]
        public static bool Verbose { get; set; }

        [Option("-vv", "Trace logging", CommandOptionType.NoValue)]
        public static bool Trace { get; set; }

        [Option("-conf|--config-file", "Config file to use", CommandOptionType.MultipleValue)]
        public string[] ConfigFiles { get; set; }

        [Option("-k|--skip-certificate-validation", "Skip SSL certificate verfification for endpoints", CommandOptionType.NoValue)]
        public bool SkipSsl { get; set; }

        protected static StarsConsoleReporter logger;

        protected IConsole _console;

        public BaseOperation(IConsole console)
        {
            this._console = console;
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

        private string GetBasePath()
        {

            using var processModule = Process.GetCurrentProcess().MainModule;
                return Path.GetDirectoryName(processModule?.FileName);
        }

        public async Task<int> OnExecuteAsync()
        {
            try
            {
                if (SkipSsl)
                {
                    ServicePointManager
                        .ServerCertificateValidationCallback +=
                        (sender, cert, chain, sslPolicyErrors) => true;
                }
                await ExecuteAsync();
            }
            catch (CommandParsingException cpe)
            {
                return StarsApp.PrintErrorAndUsage(cpe.Command, cpe.Message);
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

            logger = new StarsConsoleReporter(_console, GetVerbosityLevel());

            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            //Determines the working environment as IHostingEnvironment is unavailable in a console app
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                devEnvironmentVariable.ToLower() == "development";

            var collection = new ServiceCollection();

            // Add logging
            collection.AddLogging(c => c.AddProvider(new StarsConsoleLoggerProvider(_console, GetVerbosityLevel()))
                        .AddFilter(logLevel => logLevel > LogLevel.Debug || Verbose));

            // Add Configuration
            var builder = new ConfigurationBuilder();
            // tell the builder to look for the appsettings.json file
            builder.AddNewtonsoftJsonFile("/etc/Stars/appsettings.json", optional: true);
            foreach (var jsonFilename in Directory.EnumerateFiles(GetBasePath(), "stars-*.json", SearchOption.TopDirectoryOnly))
                    builder.AddNewtonsoftJsonFile(jsonFilename);

            if (Directory.Exists("/etc/Stars/conf.d"))
            {
                foreach (var yamlFilename in Directory.EnumerateFiles("/etc/Stars/conf.d", "*.json", SearchOption.TopDirectoryOnly))
                    builder.AddNewtonsoftJsonFile(yamlFilename);
            }
            builder.AddNewtonsoftJsonFile(Path.Join(System.Environment.GetEnvironmentVariable("HOME"), ".config", "Stars", "usersettings.json"), optional: true)
                   .AddNewtonsoftJsonFile("appsettings.json", optional: true);

            //only add secrets in development
            if (isDevelopment)
            {
                var binPath = Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().Location)).AbsolutePath);
                foreach (var jsonFilename in Directory.EnumerateFiles(binPath, "stars-*.json", SearchOption.TopDirectoryOnly))
                    builder.AddNewtonsoftJsonFile(jsonFilename);
                builder.AddNewtonsoftJsonFile("appsettings.Development.json", optional: true);
                builder.AddUserSecrets<StarsApp>();
            }
            builder.AddEnvironmentVariables();

            if (ConfigFiles != null && ConfigFiles.Count() > 0)
            {
                foreach (var file in ConfigFiles)
                {
                    var fi = new FileInfo(file);
                    if (!fi.Exists)
                        throw new FileNotFoundException("File does not exist", fi.FullName);
                    builder.AddNewtonsoftJsonFile(fi.FullName, optional: true);
                }
            }

            Configuration = builder.Build();



            collection.AddSingleton<IConfigurationRoot>(Configuration);

            // Add the command line services
            collection.AddSingleton<IConsole>(_console);
            collection.AddSingleton<ILogger>(logger);

            // Add Stars Services
            collection.AddStarsManagedServices(builder =>
            {
                builder.UseDefaultConfiguration(Configuration);
            });
            collection.LoadConfiguredStarsPlugin((assemblyPath) =>
            {
                return AssemblyLoadContext.Default;
            });
            // Use also the console for the credentials
            collection.AddSingleton<ConsoleUserSettings, ConsoleUserSettings>();
            collection.UseCredentialsManager<ConsoleCredentialsManager>();

            // Registers services specific to operation
            RegisterOperationServices(collection);

            return collection;

        }

        private int GetVerbosityLevel()
        {
            int verbosity = 0;
            if ( Verbose )
                verbosity ++;
            
            if ( Trace )
                verbosity = 2;

            return verbosity;
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