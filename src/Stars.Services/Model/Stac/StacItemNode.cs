using System.Collections.Generic;
using System.Linq;
using Stac;
using GeoJSON.Net.Geometry;
using System.Net;
using Terradue.Stars.Interface;
using System;
using Itenso.TimePeriod;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacItemNode : StacNode, IItem
    {
        public StacItemNode(StacItem stacItem, Uri uri) : base(stacItem, uri)
        {
            
        }

        public StacItem StacItem => stacObject as StacItem;

        public override ResourceType ResourceType => ResourceType.Item;

        public IGeometryObject Geometry => StacItem.Geometry;

        public IDictionary<string, object> Properties => StacItem.Properties;

        public IReadOnlyDictionary<string, IAsset> Assets => StacItem.Assets.ToDictionary(asset => asset.Key, asset => (IAsset)new StacAssetAsset(asset.Value, this));

        public ITimePeriod DateTime => StacItem.DateTime;


        public override IReadOnlyList<IResource> GetRoutes(IRouter router)
        {
            return new List<IResource>();
        }

    }
}