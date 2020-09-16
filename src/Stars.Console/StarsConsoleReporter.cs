using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Microsoft.Extensions.Logging;
using ShellProgressBar;

namespace Stars
{
    public class StarsConsoleReporter : ConsoleReporter, ILogger
    {
        public StarsConsoleReporter(IConsole console, bool verbose) : base(console, verbose, false)
        {
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

        protected void TickToCompletion(IProgressBar pbar, int ticks, int sleep = 1750, Action childAction = null)
        {
            var initialMessage = pbar.Message;
            for (var i = 0; i < ticks; i++)
            {
                pbar.Message = $"Start {i + 1} of {ticks}: {initialMessage}";
                childAction?.Invoke();
                Thread.Sleep(sleep);
                pbar.Tick($"End {i + 1} of {ticks}: {initialMessage}");
            }
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