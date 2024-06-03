using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Console.Operations;

namespace Terradue.Stars.Console
{
    [Command(Name = "Stars", FullName = "Stars", Description = "Spatio Temporal Asset Routing Services")]
    [HelpOption]
    [Subcommand(
        typeof(ListOperation),
        typeof(CopyOperation),
        typeof(PluginsOperation)

    )]
    public class StarsApp
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static int Main(string[] args)
        {
            CommandLineApplication<StarsApp> app = CreateApplication(PhysicalConsole.Singleton);

            try
            {
                PhysicalConsole.Singleton.Out.WriteLine(app.GetVersionText().Replace("\n", "/").TrimEnd('/'));
                return app.Execute(args);
            }
            catch (CommandParsingException cpe)
            {
                return PrintErrorAndUsage(cpe.Command, cpe.Message);
            }
            catch (TargetInvocationException e)
            {
                PhysicalConsole.Singleton.Error.WriteLine(e.InnerException);
                return 1;
            }
            catch (Exception e)
            {
                PhysicalConsole.Singleton.Error.WriteLine(e);
                return 1;
            }
        }

        public static CommandLineApplication<StarsApp> CreateApplication(IConsole console)
        {
            var app = new CommandLineApplication<StarsApp>(console);
            app.VersionOptionFromAssemblyAttributes(typeof(StarsApp).Assembly);
            app.Conventions.UseDefaultConventions();
            return app;
        }

        public static int PrintErrorAndUsage(CommandLineApplication command, string message)
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

        public static int OnValidationError(CommandLineApplication command, ValidationResult ve)
        {
            PhysicalConsole.Singleton.Error.WriteLine(ve.ErrorMessage);
            command.ShowHelp();
            return 1;
        }
    }
}
