using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Supplier
{
    public class GenericAsset : IAsset
    {
        private IResource route;
        private readonly string title;
        private readonly IReadOnlyList<string> roles;
        private Uri uri;
        private Dictionary<string, object> properties = new Dictionary<string, object>();

        public GenericAsset(IResource route, string title, IReadOnlyList<string> roles)
        {
            this.route = route;
            this.title = title;
            this.roles = roles;
            this.uri = route.Uri;
            if (route is IAsset)
            {
                properties = new Dictionary<string, object>((route as IAsset).Properties.ToDictionary(x => x.Key, x => x.Value));
            }
        }

        public string Title => title;

        public IReadOnlyList<string> Roles => roles;

        public Uri Uri => uri;

        public ContentType ContentType => route.ContentType;

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => route.ContentLength;

        public ContentDisposition ContentDisposition => route.ContentDisposition;

        public IReadOnlyDictionary<string, object> Properties => properties;

        public IStreamResource GetStreamable()
        {
            return route as IStreamResource;
        }

        internal void MergeProperties(IReadOnlyDictionary<string, object> props)
        {
            foreach (var key in props.Keys)
            {
                properties.Remove(key);
                properties.Add(key, props[key]);
            }
        }

        public async Task CacheHeaders(bool force = false)
        {
            if ( route is WebRoute )
                await (route as WebRoute).CacheHeadersAsync(force);
        }
    }
}