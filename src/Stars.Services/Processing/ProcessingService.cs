using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Catalog;
using Stac.Item;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;

using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Translator;
using Terradue.Stars.Services.Processing;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Supplier.Destination;
using Terradue.Stars.Services.Supplier;

namespace Terradue.Stars.Services.Processing
{
    public class ProcessingService : IStarsService
    {
        public ProcessingServiceParameters Parameters { get; set; }
        private readonly ILogger logger;
        private readonly ProcessingManager processingManager;

        public ProcessingService(ILogger logger, ProcessingManager processingManager)
        {
            this.logger = logger;
            this.processingManager = processingManager;
            Parameters = new ProcessingServiceParameters();
        }

        public async Task<IRoute> ExecuteAsync(IRoute route)
        {
            IRoute newRoute = route;
            foreach (var processing in processingManager.Plugins)
            {
                if (!processing.CanProcess(route)) continue;
                newRoute = await processing.Process(route);
            }
            return newRoute;
        }


    }
}