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

namespace Terradue.Stars.Services.Supplier.Carrier
{
    public abstract class LocalCarrier : ICarrier
    {
        protected GlobalOptions carrierServiceOptions;

        public LocalCarrier(IOptions<GlobalOptions> options)
        {
            this.carrierServiceOptions = options.Value;
        }

        public void Configure(IConfigurationSection configuration)
        {

        }

        public abstract int Priority { get; set; }
        public abstract string Key { get; set; }

        public abstract string Id { get; }

        public abstract bool CanDeliver(IResource route, IDestination destination);

        public abstract Task<IResource> Deliver(IDelivery delivery);

        private async Task StreamToFile(Stream stream, LocalFileSystemRoute localRoute)
        {
            FileInfo file = new FileInfo(localRoute.Uri.AbsolutePath);
            using (FileStream fileStream = file.Create())
            {
                await stream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }
        }

        public IDelivery QuoteDelivery(IResource route, IDestination destination)
        {
            if (!CanDeliver(route, destination)) return null;

            // Let's make a cost as MB to download
            int cost = 1000;
            if (route.ContentLength > 0)
                cost = Convert.ToInt32(route.ContentLength / 1024 / 1024);

            return new LocalDelivery(this, route, destination as LocalFileDestination, cost);
        }

        public void Configure(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {
        }
    }
}