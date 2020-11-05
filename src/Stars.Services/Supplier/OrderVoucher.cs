using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;

namespace Terradue.Stars.Services.Supplier
{
    [JsonObject]
    public class OrderVoucher : IResource, IStreamable, IOrder, IAsset
    {
        private IOrderable orderableRoute;
        private readonly string orderId;

        public OrderVoucher(IOrderable route, string orderId)
        {
            this.orderableRoute = route;
            this.orderId = orderId;
        }

        [JsonProperty]
        public Uri Uri => orderableRoute.OriginUri;

        [JsonProperty]
        public ContentType ContentType => orderableRoute.ContentType;

        [JsonProperty]
        public ResourceType ResourceType => orderableRoute.ResourceType;

        [JsonProperty]
        public ulong ContentLength => orderableRoute.ContentLength;

        [JsonProperty]
        public string SupplierType => orderableRoute.Supplier.GetType().FullName;

        [JsonIgnore]
        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = string.Format("{0}.order.json", orderId) };

        [JsonProperty]
        public string OrderId => orderId;

        [JsonIgnore]
        public ISupplier Supplier { get => orderableRoute.Supplier; }

        [JsonIgnore]
        public IOrderable OrderableRoute { get => orderableRoute; }

        [JsonIgnore]
        public string Label => string.Format("Order {0} to supplier {1}", orderId, Supplier.Id);

        [JsonIgnore]
        public IEnumerable<string> Roles => new string[1] { "order" };

        [JsonIgnore]
        public bool CanBeRanged => false;

        public async Task<Stream> GetStreamAsync()
        {
            return await Task<Stream>.Run(() =>
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                var serializer = new JsonSerializer();
                serializer.Serialize(sw, this);
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return ms as Stream;
            });
        }


        internal async Task<IOrder> Order()
        {
            return await Supplier.Order(orderableRoute);
        }

        public IStreamable GetStreamable()
        {
            return this;
        }

        public Task<Stream> GetStreamAsync(long start, long end = -1)
        {
            throw new NotImplementedException();
        }
    }
}