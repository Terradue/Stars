using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Terradue.Stars.Console
{
    public class StarsConsoleReporter : ConsoleReporter, ILogger
    {
        private readonly int verbose;

        public StarsConsoleReporter(IConsole console, int verbose) : base(console, verbose > 0, false)
        {
            this.verbose = verbose;
            // const int totalTicks = 10;
            // var options = new ProgressBarOptions
            // {
            //     ProgressCharacter = '─',
            //     ProgressBarOnBottom = true
            // };
            // using (var pbar = new ProgressBar(totalTicks, "progress bar is on the bottom now", options))
            // {
            //     Task.Run(() => TickToCompletion(pbar, totalTicks, sleep: 500));
            //     pbar.Spawn()
            // }
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

            if (logLevel == LogLevel.Trace && verbose <= 1)
                return;

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
