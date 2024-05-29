using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;
using Itenso.TimePeriod;
using Stac.Collection;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier
{
    public class CollectionContainerNode : IAssetsContainer, ICollection
    {
        private readonly ICollection collection;
        private readonly IReadOnlyDictionary<string, IAsset>  assets;
        private readonly string suffix;

        public CollectionContainerNode(ICollection collection, IReadOnlyDictionary<string, IAsset> assets, string suffix)
        {
            this.collection = collection;
            this.assets = assets;
            this.suffix = suffix;
        }

        public IResource Node => collection;

        public Uri Uri => collection.Uri;

        public ContentType ContentType => collection.ContentType;

        public ResourceType ResourceType => collection.ResourceType;

        public ulong ContentLength => collection.ContentLength;

        public string Title => collection.Title;

        public string Id => collection.Id + suffix;

        public ContentDisposition ContentDisposition => collection.ContentDisposition;

        public StacExtent Geometry => collection.Extent;

        public IDictionary<string, object> Properties => collection.Properties;

        public IReadOnlyDictionary<string, IAsset> Assets => assets;

        public StacExtent Extent => throw new NotImplementedException();

        public IReadOnlyList<IResourceLink> GetLinks()
        {
            return collection.GetLinks();
        }

        public IReadOnlyList<IResource> GetRoutes(IRouter router)
        {
            return collection.GetRoutes(router);
        }
    }
}