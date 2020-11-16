using System;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier
{
    public class LocalDelivery : IDelivery
    {
        private readonly ICarrier carrier;
        private readonly IResource route;
        private readonly LocalFileDestination fileDestination;
        private readonly int cost;

        public LocalDelivery(ICarrier carrier, IResource route, LocalFileDestination fileDestination, int cost)
        {
            this.carrier = carrier;
            this.route = route;
            this.fileDestination = fileDestination;
            this.cost = cost;
        }

        public int Cost => cost;

        public IDestination Destination => fileDestination;

        public IResource Route => route;

        public ICarrier Carrier => carrier;

        public string LocalPath => fileDestination.Uri.ToString();
    }
}