using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface.Router;

using Terradue.Stars.Services.Router;
using Terradue.Stars.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Terradue.Stars.Interface;
using System.IO;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Processing;
using Terradue.Stars.Services.Translator;
using System.Threading;

namespace Terradue.Stars.Console.Operations
{
    [Command(Name = "plugins", Description = "Manage the plugins")]
    internal class PluginsOperation : BaseOperation
    {
        private readonly CommandLineApplication app;

        public PluginsOperation(IConsole console, CommandLineApplication app): base(console)
        {
            this.app = app;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _console.Out.WriteLine(app.Parent.GetVersionText());
            _console.Out.WriteLine($"Loaded plugins");
            _console.Out.WriteLine($"--------------");
            var routersManager = ServiceProvider.GetService<RoutersManager>();
            _console.Out.WriteLine($"* Routers:");
            routersManager.GetPlugins().ToList().ForEach(p =>
            {
               _console.Out.WriteLine($"  * [{p.Key}] {p.Value.Label}");
            });
            var suppliersManager = ServiceProvider.GetService<SupplierManager>();
            _console.Out.WriteLine($"* Suppliers:");
            suppliersManager.GetPlugins().ToList().ForEach(p =>
            {
               _console.Out.WriteLine($"  * [{p.Key}] {p.Value.Label}");
            });
            var translatorManager = ServiceProvider.GetService<TranslatorManager>();
            _console.Out.WriteLine($"* Translators:");
            translatorManager.GetPlugins().ToList().ForEach(p =>
            {
               _console.Out.WriteLine($"  * [{p.Key}] {p.Value.Label}");
            });
            var processingManager = ServiceProvider.GetService<ProcessingManager>();
            _console.Out.WriteLine($"* Processings:");
            processingManager.GetPlugins().ToList().ForEach(p =>
            {
               _console.Out.WriteLine($"  * [{p.Key}] {p.Value.Label}");
            });
        }

        protected override void RegisterOperationServices(ServiceCollection collection)
        {
        }
    }
}