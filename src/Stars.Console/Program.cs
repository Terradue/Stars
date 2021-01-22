using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Console.Operations;
using System.ComponentModel.DataAnnotations;
using System;
using System.Net;

namespace Terradue.Stars.Console
{
    [Command(Name = "Stars", FullName = "Stars", Description = "Spatio Temporal Asset Routing Services")]
    [HelpOption]
    [Subcommand(
        typeof(ListOperation),
        typeof(CopyOperation)
    )]
    public class Program
    {

        public static IConfigurationRoot Configuration { get; set; }



        public static async Task<int> Main(string[] args)
        {

            

            CommandLineApplication<Program> app = new CommandLineApplication<Program>();
            app.VersionOptionFromAssemblyAttributes(typeof(Program).Assembly);

            app.Conventions.UseDefaultConventions();

            try
            {
                await PhysicalConsole.Singleton.Out.WriteLineAsync(app.GetVersionText().Replace("\n", "/").TrimEnd('/'));
                return await app.ExecuteAsync(args);
            }
            catch (CommandParsingException cpe)
            {
                return PrintErrorAndUsage(cpe.Command, cpe.Message);
            }
            catch (Exception e)
            {
                await PhysicalConsole.Singleton.Error.WriteLineAsync(e.Message);
                await PhysicalConsole.Singleton.Error.WriteLineAsync(e.StackTrace);
                return 1;
            }
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
