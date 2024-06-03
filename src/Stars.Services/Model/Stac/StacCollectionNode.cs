using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Stac;
using Stac.Collection;
using Stars.Services.Model.Stac;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacCollectionNode : StacCatalogNode, ICollection, IAssetsContainer
    {
        public StacCollectionNode(StacCollection stacCollection, Uri uri) : base(stacCollection, uri)
        {
        }

        public IReadOnlyDictionary<string, IAsset> Assets => (stacObject as StacCollection).Assets.ToDictionary(asset => asset.Key, asset => (IAsset)new StacAssetAsset(asset.Value, this));

        public StacCollection StacCollection => stacObject as StacCollection;

        public StacExtent Extent => StacCollection.Extent;

        public IDictionary<string, object> Properties => StacCollection.Properties;
    }
}