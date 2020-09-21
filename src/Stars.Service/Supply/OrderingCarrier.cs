using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Stars.Service.Router;
using System.Net.Http;
using Stars.Service.Supply.Destination;
using Stars.Interface.Supply;
using Stars.Interface.Router;
using Stars.Interface.Supply.Destination;
using Stars.Service.Supply.Carrier;

namespace Stars.Service.Supply
{
    public class OrderingCarrier : ICarrier
    {
        private readonly CarrierManager carrierManager;

        public OrderingCarrier(CarrierManager carrierManager)
        {
            this.carrierManager = carrierManager;
        }

        public void Configure(IConfigurationSection configuration)
        {
        }

        public string Id => "Ordering";

        public bool CanDeliver(IRoute route, ISupplier supplier, IDestination destination)
        {
            if (!(route is IOrderable)) return false;
            return carrierManager.GetCarriers(CreateOrderVoucher(route, supplier, "dummy"), supplier, destination).Count() > 0;
        }

        private OrderVoucher CreateOrderVoucher(IRoute route, ISupplier supplier, string orderId)
        {
            return new OrderVoucher(route as IOrderable, supplier, orderId);
        }

        public async Task<IRoute> Deliver(IDelivery delivery)
        {
            OrderedDelivery orderedDelivery = delivery as OrderedDelivery;
            IOrder order = await orderedDelivery.Supplier.Order(orderedDelivery.OrderableRoute);
            return await orderedDelivery.VoucherDelivery.Carrier.Deliver(orderedDelivery.VoucherDelivery);
        }

        public IDelivery QuoteDelivery(IRoute route, ISupplier supplier, IDestination destination)
        {
            if (!CanDeliver(route, supplier, destination)) return null;

            IOrderable orderableRoute = route as IOrderable;

            OrderVoucher orderVoucher = CreateOrderVoucher(route, supplier, orderableRoute.Id);

            // Find a carrier for the voucher
            var deliveryQuotes = carrierManager.GetSingleDeliveryQuotations(supplier, orderVoucher, destination);
            if ( deliveryQuotes == null ) return null;
            var voucherDelivery = deliveryQuotes.FirstOrDefault();
            if ( voucherDelivery == null ) return null;

            return new OrderedDelivery(this, voucherDelivery, orderVoucher, voucherDelivery.Cost + 10000);
        }

    }
}