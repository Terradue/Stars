using System;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Stars
{
    internal class StarsConsoleReporter : ConsoleReporter, ILogger
    {
        public StarsConsoleReporter(IConsole console, CommandLineApplication app) : base(console, Program.Verbose, false)
        {

        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);

            switch (logLevel)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    base.Error(message);
                    break;
                case LogLevel.Debug:
                case LogLevel.Trace:
                    base.Verbose(message);
                    break;
                case LogLevel.Information:
                    base.Output(message);
                    break;
                case LogLevel.Warning:
                    base.Warn(message);
                    break;
            }


        }
    }
}