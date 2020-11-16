using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Stac.Catalog;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;

using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacAssetAsset : IAsset
    {
        private StacAsset asset;
        private readonly ICredentials credentials;
        private readonly Uri absoluteUri;
        private readonly WebRoute resource;

        public StacAssetAsset(StacAsset asset, StacItemNode parent, System.Net.ICredentials credentials = null)
        {
            this.asset = asset;
            this.credentials = credentials;
            this.absoluteUri = asset.Uri.IsAbsoluteUri ? asset.Uri : new Uri(parent.Uri, asset.Uri);
            this.resource = WebRoute.Create(Uri, ContentLength, credentials);
        }

        public Uri Uri => absoluteUri;

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

        public ContentDisposition ContentDisposition => resource.ContentDisposition ?? new ContentDisposition() { FileName = Filename };

        public IStreamable GetStreamable()
        {
            return resource;
        }
    }
}