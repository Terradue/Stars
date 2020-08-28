using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace Stars
{
    internal class StarsConsoleReporter : ConsoleReporter
    {
        public StarsConsoleReporter(IConsole console, CommandLineApplication app) : base(console, Program.Verbose, false)
        {
            
        }
    }
}