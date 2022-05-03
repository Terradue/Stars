using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Stac.Extensions.Alternate;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacAlternateAssetAsset : IAsset
    {
        private readonly StacAssetAsset parentAsset;
        private readonly AlternateAssetObject alternateAsset;
        private readonly Task<IStreamable> _streamable;
        private readonly ICredentials credentials;
        private readonly Uri uri;

        public StacAlternateAssetAsset(AlternateAssetObject alternateAsset, StacAssetAsset parentAsset, ICredentials credentials = null)
        {
            this.alternateAsset = alternateAsset;
            this.parentAsset = parentAsset;
            this.credentials = credentials;
            if (alternateAsset.Uri.IsAbsoluteUri)
                this.uri = alternateAsset.Uri;
            else
            {
                if (parentAsset?.Parent != null)
                {
                    if (parentAsset.Parent.Uri.IsAbsoluteUri)
                        this.uri = new Uri(parentAsset.Parent.Uri, alternateAsset.Uri);
                    else
                        this.uri = new Uri(parentAsset.Parent.Uri.ToString() + "/" + alternateAsset.Uri.ToString(), UriKind.Relative);
                }
                else this.uri = alternateAsset.Uri;
            }
            if (alternateAsset is IStreamable)
                _streamable = Task.FromResult(alternateAsset as IStreamable);
            else
            {
                _streamable = WebRoute.Create(uri, credentials: credentials);
            }
        }

        public Uri Uri => uri;

        public ContentType ContentType => parentAsset.ContentType;

        public ulong ContentLength
        {
            get
            {
                var cl = _streamable?.ContentLength;
                if (cl.HasValue) return cl.Value;
                return parentAsset.ContentLength;
            }
        }
        public string Title => alternateAsset.Title;

        public ResourceType ResourceType => ResourceType.Asset;

        public string Filename
        {
            get
            {
                if (alternateAsset.Properties.ContainsKey("filename"))
                    return alternateAsset.GetProperty<string>("filename");
                return parentAsset?.Filename;
            }
        }

        public IReadOnlyList<string> Roles => parentAsset?.Roles.ToList();

        public StacAsset StacAsset { get => parentAsset.StacAsset; }

        public ContentDisposition ContentDisposition => parentAsset.ContentDisposition;

        public IReadOnlyDictionary<string, object> Properties => new ReadOnlyDictionary<string, object>(alternateAsset.Properties);

        public IEnumerable<IAsset> Alternates => Enumerable.Empty<IAsset>();

        public async Task CacheHeaders(bool force = false)
        {
            if (_streamable is WebRoute)
                await (_streamable as WebRoute).CacheHeadersAsync(force);
        }

        public IStreamable GetStreamable()
        {
            return _streamable;
        }
    }
}