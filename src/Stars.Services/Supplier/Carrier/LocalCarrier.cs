using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Services.Router;
using System.Net.Http;
using Terradue.Stars.Services.Supplier.Destination;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface;
using Microsoft.Extensions.Logging;

namespace Terradue.Stars.Services.Supplier.Carrier
{
    public abstract class LocalCarrier : ICarrier
    {
        private readonly ILogger logger;

        protected readonly System.IO.Abstractions.IFileSystem fileSystem;
        public LocalCarrier(ILogger logger, System.IO.Abstractions.IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
            this.logger = logger;
        }

        public abstract int Priority { get; set; }
        public abstract string Key { get; set; }

        public abstract string Id { get; }

        public abstract bool CanDeliver(IResource route, IDestination destination);

        public abstract Task<IResource> Deliver(IDelivery delivery, bool overwrite = false);

        public IDelivery QuoteDelivery(IResource route, IDestination destination)
        {
            if (!CanDeliver(route, destination)) return null;

            // Let's make a cost as MB to download
            int cost = 1000;
            try
            {
                if (route.ContentLength > 0)
                    cost = Convert.ToInt32(route.ContentLength / 1024 / 1024);
            }
            catch (Exception e)
            {
                logger.LogWarning("Error trying to get size of the node {0} : {1}", route.Uri, e.Message);
            }

            return new LocalDelivery(this, route, destination as LocalFileDestination, cost);
        }
    }
}