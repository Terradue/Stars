using System;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier
{
    internal class NoDelivery : IDelivery
    {
        private IRoute route;
        private ISupplier supplier;
        private readonly ICarrier carrier;

        public NoDelivery(IRoute route, ISupplier supplier, ICarrier carrier)
        {
            this.route = route;
            this.supplier = supplier;
            this.carrier = carrier;
        }

        public int Cost => 10000;

        public IRoute Route => route;

        public ICarrier Carrier => carrier;

        public ISupplier Supplier => supplier;

        public Uri TargetUri => route.Uri;
    }
}