using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Stac.Catalog;
using Stars.Interface.Router;
using Stars.Interface.Supply.Asset;
using Stars.Service.Router;

namespace Stars.Service.Model.Stac
{
    internal class StacAssetAsset : IAsset
    {
        private StacAsset asset;
        private readonly IStacObject stacObject;

        public StacAssetAsset(StacAsset asset, IStacObject stacObject)
        {
            this.asset = asset;
            this.stacObject = stacObject;
        }

        public Uri Uri => asset.Uri;

        public ContentType ContentType => asset.MediaType;

        public ulong ContentLength => asset.ContentLength;

        public string Label
        {
            get
            {
                string label = "";
                if (asset.Roles != null && asset.Roles.Count() > 0)
                    label += string.Format("[{0}] ", string.Join(",", asset.Roles));
                label += string.IsNullOrEmpty(asset.Title) ? Path.GetFileName(asset.Uri.AbsolutePath) : asset.Title;
                return label;
            }
        }

        public ResourceType ResourceType => ResourceType.Asset;

        public string Filename => Path.GetFileName(Uri.ToString());

        public IEnumerable<string> Roles => asset.Roles;

        public async Task<INode> GoToNode()
        {
            return await WebRoute.Create(Uri).GoToNode();
        }

        public IStreamable GetStreamable()
        {
            return WebRoute.Create(Uri);
        }
    }
}