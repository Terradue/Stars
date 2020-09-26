using System;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Interface.Supply.Destination;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supply.Destination;

namespace Terradue.Stars.Services.Supply
{
    public class LocalDelivery : IDelivery
    {
        private readonly ICarrier carrier;
        private readonly IRoute route;
        private readonly ISupplier supplier;
        private readonly LocalDirectoryDestination dirDestination;
        private readonly string localPath;
        private readonly int cost;

        public LocalDelivery(ICarrier carrier, IRoute route, ISupplier supplier, LocalDirectoryDestination dirDestination, string localPath, int cost)
        {
            this.carrier = carrier;
            this.route = route;
            this.supplier = supplier;
            this.dirDestination = dirDestination;
            this.localPath = localPath;
            this.cost = cost;
        }

        public int Cost => cost;

        public IDestination Destination => dirDestination;

        public IRoute Route => route;

        public ICarrier Carrier => carrier;

        public ISupplier Supplier => supplier;

        public Uri TargetUri => new Uri(localPath);

        public string LocalPath => localPath;
    }
}