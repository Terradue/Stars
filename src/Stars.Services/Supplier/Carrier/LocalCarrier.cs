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
using Terradue.Stars.Services.Processing.Destination;
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Destination;
using Microsoft.Extensions.Options;

namespace Terradue.Stars.Services.Processing.Carrier
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

        public abstract string Id { get; }

        public abstract bool CanDeliver(IRoute route, ISupplier supplier, IDestination destination);

        public abstract Task<IRoute> Deliver(IDelivery delivery);

        private async Task StreamToFile(Stream stream, LocalFileSystemRoute localRoute)
        {
            FileInfo file = new FileInfo(localRoute.Uri.AbsolutePath);
            using (FileStream fileStream = file.Create())
            {
                await stream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }
        }

        public IDelivery QuoteDelivery(IRoute route, ISupplier supplier, IDestination destination)
        {
            if (!CanDeliver(route, supplier, destination)) return null;

            LocalDirectoryDestination directory = (LocalDirectoryDestination)destination;

            var localFile = FindLocalDestination(route, directory);

            // Let's make a cost as MB to download
            int cost = 1000;
            if (route.ContentLength > 0)
                cost = Convert.ToInt32(route.ContentLength / 1024 / 1024);
            else if (localFile.Item2 > 0) {
                cost = Convert.ToInt32(localFile.Item2 / 1024 / 1024);
            }

            return new LocalDelivery(this, route, supplier, directory, localFile.Item1, cost);
        }

        protected abstract (string, ulong) FindLocalDestination(IRoute route, LocalDirectoryDestination directory);

        public void Configure(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {
        }
    }
}