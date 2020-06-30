using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace Stars
{
    internal class StacConsoleReporter : ConsoleReporter
    {
        public StacConsoleReporter(IConsole console, CommandLineApplication app) : base(console, Program.Verbose, false)
        {
            
        }
    }
}