using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Stac.Catalog;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Asset;
using Terradue.Stars.Service.Router;

namespace Terradue.Stars.Service.Model.Stac
{
    public class StacAssetAsset : IAsset
    {
        private StacAsset asset;

        public StacAssetAsset(StacAsset asset)
        {
            this.asset = asset;
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

        public StacAsset StacAsset { get => asset; }

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