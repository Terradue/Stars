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
using Microsoft.Extensions.Logging;

namespace Terradue.Stars.Services.Supplier.Carrier
{
    public class NativeCarrier : ICarrier
    {

        private readonly ILogger logger;

        public NativeCarrier(IOptions<GlobalOptions> options, ILogger logger) 
        {
            this.logger = logger;
        }

        public string Id => "Native";

        public bool CanDeliver(IRoute route, ISupplier supplier, IDestination destination)
        {
            return (route != null);
        }

        public void Configure(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {
        }

        public Task<IRoute> Deliver(IDelivery delivery)
        {
            return Task.FromResult<IRoute>(delivery.Route);
        }

        public IDelivery QuoteDelivery(IRoute route, ISupplier supplier, IDestination destination)
        {
            return new NoDelivery(route, supplier, this);
        }
    }
}