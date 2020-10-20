using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Microsoft.Extensions.Logging;

namespace Terradue.Stars.Console
{
    public class StarsConsoleLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, StarsConsoleReporter> _loggers = new ConcurrentDictionary<string, StarsConsoleReporter>();
        private readonly IConsole console;
        private readonly bool verbose;

        public StarsConsoleLoggerProvider(IConsole console, bool verbose)
        {
            this.console = console;
            this.verbose = verbose;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new StarsConsoleReporter(console, verbose));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}