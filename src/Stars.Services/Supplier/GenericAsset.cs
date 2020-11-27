using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Services.Supplier
{
    internal class GenericAsset : IAsset
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
        }

        public string Title => title;

        public IReadOnlyList<string> Roles => roles;

        public Uri Uri => uri;

        public ContentType ContentType => route.ContentType;

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => route.ContentLength;

        public ContentDisposition ContentDisposition => route.ContentDisposition;

        public IReadOnlyDictionary<string, object> Properties => properties;

        public IStreamable GetStreamable()
        {
            return route as IStreamable;
        }
    }
}