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
using Terradue.Stars.Services.Supplier.Destination;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Interface;
using System.Threading;

namespace Terradue.Stars.Services.Supplier.Carrier
{
    public class OrderingCarrier : ICarrier
    {
        private readonly CarrierManager carrierManager;

        public OrderingCarrier(CarrierManager carrierManager)
        {
            this.carrierManager = carrierManager;
        }

        public int Priority { get; set; }
        public string Key { get => Id; set { } }

        public string Id => "Ordering";

        public bool CanDeliver(IResource route, IDestination destination)
        {
            if (!(route is IOrderable)) return false;
            return carrierManager.GetCarriers(CreateOrderVoucher(route as IOrderable, "dummy"), destination).Count() > 0;
        }

        private OrderVoucher CreateOrderVoucher(IOrderable route, string orderId)
        {
            return new OrderVoucher(route, orderId);
        }

        public async Task<IResource> DeliverAsync(IDelivery delivery, CancellationToken ct, bool overwrite = false)
        {
            OrderedDelivery orderedDelivery = delivery as OrderedDelivery;
            IOrder order = await orderedDelivery.Supplier.Order(orderedDelivery.OrderableRoute);
            return await orderedDelivery.VoucherDelivery.Carrier.DeliverAsync(orderedDelivery.VoucherDelivery, ct, overwrite);
        }

        public IDelivery QuoteDelivery(IResource route, IDestination destination)
        {
            if (!CanDeliver(route, destination)) return null;

            IOrderable orderableRoute = route as IOrderable;

            OrderVoucher orderVoucher = CreateOrderVoucher(orderableRoute, orderableRoute.Id);

            // Find a carrier for the voucher
            var deliveryQuotes = carrierManager.GetSingleDeliveryQuotations(orderVoucher, destination.To(orderVoucher, orderableRoute.Id + "/orders/"));
            if (deliveryQuotes == null) return null;
            var voucherDelivery = deliveryQuotes.FirstOrDefault();
            if (voucherDelivery == null) return null;

            return new OrderedDelivery(this, voucherDelivery, orderVoucher, voucherDelivery.Cost + 10000);
        }

    }
}