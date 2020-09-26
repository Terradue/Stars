using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.ServiceModel.Syndication;

namespace Terradue.Stars.Services.Model.Atom
{
    internal class AtomItemRoute : IRoute
    {
        private SyndicationItem item;
        private SyndicationFeed feed;

        public AtomItemRoute(SyndicationItem item, SyndicationFeed feed)
        {
            this.item = item;
            this.feed = feed;
        }

        public Uri Uri => item.Links.FirstOrDefault(link => link.RelationshipType == "self").Uri;

        public ContentType ContentType => new ContentType("application/atom+xml");

        public bool CanRead => true;

        public ResourceType ResourceType => ResourceType.Item;

        public ulong ContentLength => 0;

        public Task<INode> GoToNode()
        {
            return Task<INode>.FromResult((INode)new AtomItemRoutable(item));
        }
    }
}