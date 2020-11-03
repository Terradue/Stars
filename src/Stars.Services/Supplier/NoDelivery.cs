using System;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier
{
    internal class NoDelivery : IDelivery
    {
        private IRoute route;
        private readonly ICarrier carrier;

        public NoDelivery(IRoute route, ICarrier carrier)
        {
            this.route = route;
            this.carrier = carrier;
        }

        public int Cost => 100000;

        public IRoute Route => route;

        public ICarrier Carrier => carrier;

        public IDestination Destination => null;
    }
}