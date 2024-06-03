using System;
using System.Collections.Generic;
using System.Net.Mime;
using GeoJSON.Net.Geometry;
using Itenso.TimePeriod;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Supplier
{
    public class ItemContainerNode : IAssetsContainer, IItem
    {
        private readonly IItem item;
        private readonly IReadOnlyDictionary<string, IAsset> assets;
        private readonly string suffix;

        public ItemContainerNode(IItem item, IReadOnlyDictionary<string, IAsset> assets, string suffix)
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

        public string Title => item.Title;

        public string Id => item.Id + suffix;

        public ContentDisposition ContentDisposition => item.ContentDisposition;

        public IGeometryObject Geometry => item.Geometry;

        public IDictionary<string, object> Properties => item.Properties;

        public IReadOnlyDictionary<string, IAsset> Assets => assets;

        public ITimePeriod DateTime => item.DateTime;

        public IReadOnlyList<IResourceLink> GetLinks()
        {
            return item.GetLinks();
        }
    }
}
