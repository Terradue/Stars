using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Interface.Supply.Destination;
using Stars.Service.Router;
using Stars.Service.Supply.Destination;

namespace Stars.Service.Supply
{
    public class SimpleDelivery : IDelivery
    {
        private readonly ICarrier carrier;
        private readonly IRoute route;
        private readonly ISupplier supplier;
        private readonly IDestination destination;
        private readonly int cost;

        public SimpleDelivery(ICarrier carrier, IRoute route, ISupplier supplier, IDestination destination, int cost)
        {
            this.carrier = carrier;
            this.route = route;
            this.supplier = supplier;
            this.destination = destination;
            this.cost = cost;
        }

        public int Cost => cost;

        public IDestination Destination => destination;

        public IRoute Route => route;

        public ICarrier Carrier => carrier;

        public ISupplier Supplier => supplier;
    }
}