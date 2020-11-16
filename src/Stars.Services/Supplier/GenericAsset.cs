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
        private readonly string label;
        private readonly IEnumerable<string> roles;
        private Uri uri;

        public GenericAsset(IResource route, string label, IEnumerable<string> roles)
        {
            this.route = route;
            this.label = label;
            this.roles = roles;
            this.uri = route.Uri;
        }

        public string Label => label;

        public IEnumerable<string> Roles => roles;

        public Uri Uri => uri;

        public ContentType ContentType => route.ContentType;

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => route.ContentLength;

        public ContentDisposition ContentDisposition => route.ContentDisposition;

        public IStreamable GetStreamable()
        {
            return route as IStreamable;
        }
    }
}