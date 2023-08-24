// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StarsConsoleLoggerProvider.cs

using System;
using System.Collections.Concurrent;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Terradue.Stars.Console
{
    public class StarsConsoleLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, StarsConsoleReporter> _loggers = new();
        private readonly IConsole console;
        private readonly int verbose;

        public StarsConsoleLoggerProvider(IConsole console, int verbose)
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
