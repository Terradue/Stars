using System;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Supplier.Carrier;

namespace Terradue.Stars.Services.Supplier
{
    internal class OrderedDelivery : IDelivery
    {

        public OrderedDelivery(OrderingCarrier orderingCarrier, IDelivery voucherDelivery, OrderVoucher orderVoucher, int cost)
        {
            Carrier = orderingCarrier;
            VoucherDelivery = voucherDelivery;
            OrderVoucher = orderVoucher;
            Cost = cost;
        }

        public ICarrier Carrier { get; }
        public IDelivery VoucherDelivery { get; }
        public OrderVoucher OrderVoucher { get; }
        public IResource Resource => OrderVoucher;
        public ISupplier Supplier => OrderVoucher.Supplier;
        public int Cost { get; }

        public IOrderable OrderableRoute => OrderVoucher.OrderableRoute;

        public IDestination Destination => VoucherDelivery.Destination;

        public override string ToString() => string.Format("[Order] {0}: {1} for {2}$", Supplier.Id, OrderVoucher.OrderId, Cost);

        public Func<IDelivery, IResource, Task> PreCheckDeliveryFunction { get; set; }
    }
}
