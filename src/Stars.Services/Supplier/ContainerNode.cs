using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier
{
    public class ContainerNode : IAssetsContainer, IItem, IStreamable
    {
        private readonly IItem item;
        private readonly IDictionary<string, IAsset>  assets;

        public ContainerNode(IItem item, IDictionary<string, IAsset>  assets)
        {
            this.item = item;
            this.assets = assets;
        }

        public IRoute Node => item;

        public Uri Uri => item.Uri;

        public ContentType ContentType => item.ContentType;

        public ResourceType ResourceType => item.ResourceType;

        public ulong ContentLength => item.ContentLength;

        public string Label => item.Label;

        public string Id => item.Id;

        public ContentDisposition ContentDisposition => item.ContentDisposition;

        public IGeometryObject Geometry => item.Geometry;

        public IDictionary<string, object> Properties => item.Properties;

        public IDictionary<string, IAsset> GetAssets() => assets;

        public async Task<Stream> GetStreamAsync()
        {
            return await item.GetStreamAsync();
        }
    }
}