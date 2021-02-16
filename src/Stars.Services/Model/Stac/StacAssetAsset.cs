using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private WebRoute webRoute;
        private readonly ICredentials credentials;
        private readonly Uri uri;

        public StacAssetAsset(StacAsset asset, StacItemNode parent, System.Net.ICredentials credentials = null)
        {
            this.asset = asset;
            this.credentials = credentials;
            if (asset.Uri.IsAbsoluteUri)
                this.uri = asset.Uri;
            else
            {
                if (parent != null)
                {
                    if (parent.Uri.IsAbsoluteUri)
                        this.uri = new Uri(parent.Uri, asset.Uri);
                    else
                        this.uri = new Uri(parent.Uri.ToString() + "/" + asset.Uri.ToString(), UriKind.Relative);
                }
                else this.uri = asset.Uri;
            }
        }

        public Uri Uri => uri;

        public ContentType ContentType => asset.MediaType;

        public ulong ContentLength
        {
            get
            {
                if (asset.ContentLength > 0) return asset.ContentLength;
                var cl = GetStreamable()?.ContentLength;
                if (cl.HasValue) return cl.Value;
                return 0;
            }
        }
        public string Title => asset.Title;

        public ResourceType ResourceType => ResourceType.Asset;

        public string Filename
        {
            get
            {
                if (asset.Properties.ContainsKey("filename"))
                    return asset.GetProperty<string>("filename");
                return Path.GetFileName(Uri.ToString());
            }
        }

        public IReadOnlyList<string> Roles => asset.Roles;

        public StacAsset StacAsset { get => asset; }

        public ContentDisposition ContentDisposition
        {
            get
            {
                var cd = GetStreamable()?.ContentDisposition ?? new ContentDisposition() { FileName = Filename };
                if (asset.Properties.ContainsKey("filename"))
                    cd.FileName = asset.GetProperty<string>("filename");
                return cd;
            }
        }

        public IReadOnlyDictionary<string, object> Properties => new ReadOnlyDictionary<string, object>(asset.Properties);

        public IStreamable GetStreamable()
        {
            if (asset is IStreamable)
                return asset as IStreamable;

            return WebRoute;

        }

        public WebRoute WebRoute
        {
            get
            {
                if (uri.IsAbsoluteUri)
                {
                    if (webRoute == null)
                    {
                        // try
                        // {
                        webRoute = WebRoute.Create(uri, asset.ContentLength, credentials);
                        // }
                        // catch { }
                    }
                    return webRoute;
                }
                return null;
            }
        }

        public async Task Remove()
        {
            if (WebRoute != null)
                await WebRoute.Remove();
        }
    }
}