using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Operations;
using System.ComponentModel.DataAnnotations;

namespace Terradue.Stars
{
    [Command(Name = "Stars", Description = "Spatio Temporal Asset Routing Services")]
    [HelpOption]
    [Subcommand(
        typeof(ListOperation),
        typeof(CopyOperation)
    )]
    public class Program
    {

        public static IConfigurationRoot Configuration { get; set; }

        public static StarsConsoleReporter _logger;

        [Option]
        public static bool Verbose { get; set; }

        static async Task<int> Main(string[] args)
        {
            CommandLineApplication<Program> app = new CommandLineApplication<Program>();

            app.Conventions.UseDefaultConventions();

            try
            {
                return await app.ExecuteAsync(args);
            }
            catch (CommandParsingException cpe)
            {
                return PrintErrorAndUsage(cpe.Command, cpe.Message);
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
