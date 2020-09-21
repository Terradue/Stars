using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stars.Interface.Router;
using Stars.Interface.Supply;

namespace Stars.Service.Supply
{
    [JsonObject]
    public class OrderVoucher : IRoute, IStreamable, IOrder
    {
        private IOrderable orderableRoute;
        private ISupplier supplier;
        private readonly string orderId;

        public OrderVoucher(IOrderable route, ISupplier supplier, string orderId)
        {
            this.orderableRoute = route;
            this.supplier = supplier;
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
        public string SupplierType => supplier.GetType().FullName;

        [JsonIgnore]
        public ContentDisposition ContentDisposition => new ContentDisposition(){ FileName = string.Format("{0}.order.json", orderId) };

        [JsonProperty]
        public string OrderId => orderId;

        public ISupplier Supplier { get => supplier; set => supplier = value; }

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

        public Task<INode> GoToNode()
        {
            throw new NotImplementedException();
        }

        internal async Task<IOrder> Order()
        {
            return await supplier.Order(orderableRoute);
        }
    }
}