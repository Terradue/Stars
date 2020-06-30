using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Stac.Catalog;
using Stars.Router;

namespace Stars.Model.Stac
{
    internal class StacAssetRoute : IRoute
    {
        private StacAsset asset;
        private readonly IStacObject stacObject;

        public StacAssetRoute(StacAsset asset, IStacObject stacObject)
        {
            this.asset = asset;
            this.stacObject = stacObject;
        }

        public Uri Uri => asset.Uri;

        public ContentType ContentType
        {
            get
            {
                if (string.IsNullOrEmpty(asset.MediaType))
                    return null;
                return new ContentType(asset.MediaType);
            }
        }

        public bool CanGetResource => false;

        public ResourceType ResourceType => ResourceType.Asset;

        public Task<IResource> GetResource()
        {
            return Task<IResource>.FromResult((IResource)null);
        }
    }
}