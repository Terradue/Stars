using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Stars
{
    internal class StarsCommandLineTool : IStarsService
    {
        private readonly CommandLineApplication app;
        private readonly IConsole console;

        public StarsCommandLineTool(CommandLineApplication app, IConsole console)
        {
            this.app = app;
            this.console = console;
        }

        public async Task InvokeAsync()
        {
            await console.Error.WriteLineAsync("Specify a subcommand");
            app.ShowHelp();
        }
    }
}