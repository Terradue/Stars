using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Interface.Supply.Destination;

namespace Stars.Service.Supply
{
    internal class OrderedDelivery : SimpleDelivery, IDelivery
    {
        public OrderedDelivery(OrderingCarrier orderingCarrier, ICarrier carrier, IRoute route, ISupplier supplier, IDestination destination, int cost) : base(orderingCarrier, route, supplier, destination, cost)
        {
            OrderVoucherCarrier = carrier;
        }

        public ICarrier OrderVoucherCarrier { get; }
        public IOrderable OrderableRoute => Route as IOrderable;
    }
}