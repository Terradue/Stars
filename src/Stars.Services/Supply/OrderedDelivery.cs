using System;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Interface.Supply.Destination;

namespace Terradue.Stars.Services.Supply
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
        public IRoute Route => OrderVoucher;
        public ISupplier Supplier => OrderVoucher.Supplier;
        public int Cost { get; }

        public IOrderable OrderableRoute => OrderVoucher.OrderableRoute;

        public Uri TargetUri => VoucherDelivery.TargetUri;

        public override string ToString() => string.Format("[Order] {0}: {1} for {2}$", Supplier.Id, OrderVoucher.OrderId, Cost);
    }
}