// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: NativeCarrier.cs

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;

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
        public string Key { get => "Native"; set { } }

        public string Id => "Native";

        public bool CanDeliver(IResource route, IDestination destination)
        {
            return (route != null);
        }

        public Task<IResource> DeliverAsync(IDelivery delivery, CancellationToken ct, bool overwrite = false)
        {
            return Task.FromResult(delivery.Resource);
        }

        public IDelivery QuoteDelivery(IResource route, IDestination destination)
        {
            return new NoDelivery(route, this);
        }
    }
}
