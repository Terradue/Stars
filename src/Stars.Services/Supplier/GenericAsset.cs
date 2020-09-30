using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Asset;

namespace Terradue.Stars.Services.Processing
{
    internal class GenericAsset : IAsset
    {
        private IRoute route;
        private readonly string label;
        private readonly IEnumerable<string> roles;

        public GenericAsset(IRoute route, string label, IEnumerable<string> roles)
        {
            this.route = route;
            this.label = label;
            this.roles = roles;
        }

        public string Label => label;

        public IEnumerable<string> Roles => roles;

        public Uri Uri => route.Uri;

        public ContentType ContentType => route.ContentType;

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => route.ContentLength;

        public async Task<INode> GoToNode()
        {
            return await route.GoToNode();
        }

        public IStreamable GetStreamable()
        {
            return route as IStreamable;
        }
    }
}