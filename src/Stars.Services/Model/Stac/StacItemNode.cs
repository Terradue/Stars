using System.Collections.Generic;
using System.Linq;
using Stac;
using GeoJSON.Net.Geometry;
using System.Net;
using Terradue.Stars.Interface;
using System;
using Itenso.TimePeriod;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacItemNode : StacNode, IItem
    {
        public StacItemNode(StacItem stacItem, Uri uri, ICredentials credentials = null) : base(stacItem, uri, credentials)
        {
            
        }

        public StacItem StacItem => stacObject as StacItem;

        public override ResourceType ResourceType => ResourceType.Item;

        public IGeometryObject Geometry => StacItem.Geometry;

        public IDictionary<string, object> Properties => StacItem.Properties;

        public IReadOnlyDictionary<string, IAsset> Assets => StacItem.Assets.ToDictionary(asset => asset.Key, asset => (IAsset)new StacAssetAsset(asset.Value, this, credentials));

        public ITimePeriod DateTime => StacItem.DateTime;

        public override IReadOnlyList<IResource> GetRoutes(ICredentials credentials)
        {
            return new List<IResource>();
        }

        public override object Clone()
        {
            return new StacItemNode(this.StacItem.Clone() as StacItem, new Uri(this.Uri.ToString()), credentials);
        }
    }
}