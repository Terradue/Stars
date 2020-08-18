using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Stars.Router;
using Terradue.ServiceModel.Syndication;

namespace Stars.Model.Atom
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

        public Task<IResource> GotoResource()
        {
            return Task<IResource>.FromResult((IResource)new AtomItemRoutable(item));
        }
    }
}