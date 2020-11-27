using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier
{
    public class ContainerNode : IAssetsContainer, IItem, IStreamable
    {
        private readonly IItem item;
        private readonly IDictionary<string, IAsset>  assets;
        private readonly string suffix;

        public ContainerNode(IItem item, IDictionary<string, IAsset> assets, string suffix)
        {
            this.item = item;
            this.assets = assets;
            this.suffix = suffix;
        }

        public IResource Node => item;

        public Uri Uri => item.Uri;

        public ContentType ContentType => item.ContentType;

        public ResourceType ResourceType => item.ResourceType;

        public ulong ContentLength => item.ContentLength;

        public string Label => item.Label;

        public string Id => item.Id + suffix;

        public ContentDisposition ContentDisposition => item.ContentDisposition;

        public IGeometryObject Geometry => item.Geometry;

        public IDictionary<string, object> Properties => item.Properties;

        public bool CanBeRanged => item.CanBeRanged;

        public IReadOnlyDictionary<string, IAsset> Assets => new ReadOnlyDictionary<string, IAsset>(assets);

        public async Task<Stream> GetStreamAsync()
        {
            return await item.GetStreamAsync();
        }

        public async Task<Stream> GetStreamAsync(long start, long end = -1)
        {
            return await item.GetStreamAsync(start, end);
        }
    }
}