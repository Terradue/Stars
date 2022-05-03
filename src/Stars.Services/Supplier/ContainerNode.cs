using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;
using Itenso.TimePeriod;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier
{
    public class ContainerNode : IAssetsContainer, IItem
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

        public IReadOnlyDictionary<string, IAsset> Assets => new ReadOnlyDictionary<string, IAsset>(assets);

        public ITimePeriod DateTime => item.DateTime;

        public IReadOnlyList<IResourceLink> GetLinks()
        {
            return item.GetLinks();
        }
    }
}