using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Stac.Catalog;
using Terradue.Stars.Interface.Router;

using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacAssetAsset : IAsset
    {
        private StacAsset asset;
        private readonly ICredentials credentials;

        public StacAssetAsset(StacAsset asset, System.Net.ICredentials credentials = null)
        {
            this.asset = asset;
            this.credentials = credentials;
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

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Filename };
        public IStreamable GetStreamable()
        {
            return WebRoute.Create(Uri, ContentLength, credentials);
        }
    }
}