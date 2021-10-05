using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.OpenSearch.DataHub;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Routers
{
    internal class OrderableAsset : IAsset, IOrderable
    {
        private readonly IOpenSearchResultItem osItem;
        private readonly ISupplier supplier;
        private Dictionary<string, object> properties;

        public OrderableAsset(IOpenSearchResultItem osItem, ISupplier supplier)
        {
            this.osItem = osItem;
            this.supplier = supplier;
            this.properties = new Dictionary<string, object>();
        }

        public string Title => osItem.Title != null ? osItem.Title.Text : osItem.Id;

        public Uri Uri => new Uri("order://" + osItem.Identifier);

        public Uri OriginUri
        {
            get
            {
                if (SelfLink != null) return SelfLink.Uri;
                if (Enclosure != null) return Enclosure.Uri;
                return null;
            }
        }

        public SyndicationLink SelfLink => osItem.Links.FirstOrDefault(l => l.RelationshipType == "self");

        public SyndicationLink Enclosure => osItem.Links.FirstOrDefault(l => l.RelationshipType == "enclosure" && l.Uri != null);

        public ContentType ContentType => new ContentType("application/octet-stream");

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => Enclosure != null ? Convert.ToUInt64(Enclosure.Length) : 0;

        public IOpenSearchResultItem OpenSearchResultItem => osItem;

        public string Id => osItem.Identifier;

        public IReadOnlyList<string> Roles => new string[] { "order" };

        public ContentDisposition ContentDisposition => null;

        public ISupplier Supplier => supplier;

        public IReadOnlyDictionary<string, object> Properties => properties;

        public IStreamable GetStreamable()
        {
            return null;
        }

        public Task Remove()
        {
            throw new NotImplementedException();
        }
    }
}