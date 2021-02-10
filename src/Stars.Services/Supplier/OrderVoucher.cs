using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Mime;
using System.Text;
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

        [JsonIgnore]
        public Uri Uri => new Uri(orderId + ".json", UriKind.Relative);

        [JsonProperty]
        public Uri OrderedItemUri => orderableRoute.OriginUri;

        [JsonProperty]
        public string OrderedItemId => orderableRoute.Id;

        [JsonIgnore]
        public ContentType ContentType => new ContentType("application/json");

        [JsonIgnore]
        public ResourceType ResourceType => ResourceType.Asset;

        [JsonIgnore]
        public ulong ContentLength => Convert.ToUInt64(Encoding.Default.GetBytes(JsonConvert.SerializeObject(this)).Length);

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

        [JsonProperty]
        public string Title => string.Format("Order {0} to supplier {1}", orderId, Supplier.Id);

        [JsonIgnore]
        public IReadOnlyList<string> Roles => new string[1] { "order" };

        [JsonIgnore]
        public bool CanBeRanged => false;

        [JsonProperty]
        public IReadOnlyDictionary<string, object> Properties => new Dictionary<string, object>();

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

        public Task Remove()
        {
            throw new NotImplementedException();
        }
    }
}