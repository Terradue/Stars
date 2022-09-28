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
using Terradue.Stars.Interface;
using System.Threading;

namespace Terradue.Stars.Services.Supplier.Carrier
{
    public class NativeCarrier : ICarrier
    {

        private readonly ILogger logger;

        public NativeCarrier(IOptions<GlobalOptions> options, ILogger logger) 
        {
            this.logger = logger;
            Priority = 10000;
        }

        public int Priority { get; set; }
        public string Key { get => "Native"; set {} }

        public string Id => "Native";

        public bool CanDeliver(IResource route, IDestination destination)
        {
            return (route != null);
        }

        public Task<IResource> DeliverAsync(IDelivery delivery, CancellationToken ct, bool overwrite = false)
        {
            return Task.FromResult<IResource>(delivery.Resource);
        }

        public IDelivery QuoteDelivery(IResource route, IDestination destination)
        {
            return new NoDelivery(route, this);
        }
    }
}