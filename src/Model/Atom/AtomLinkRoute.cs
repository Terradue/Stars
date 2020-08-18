using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Stars.Router;
using Terradue.ServiceModel.Syndication;

namespace Stars.Model.Atom
{
    internal class AtomLinkRoute : IRoute
    {
        private SyndicationLink link;
        private SyndicationItem item;

        public AtomLinkRoute(SyndicationLink link, SyndicationItem item)
        {
            this.link = link;
            this.item = item;
        }

        public Uri Uri => link.Uri;

        public ContentType ContentType
        {
            get
            {
                try
                {
                    return new ContentType(link.MediaType);
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool CanRead => false;

        public ResourceType ResourceType {
            get {
                switch (link.RelationshipType){
                    case "enclosure":
                    case "icon":
                        return ResourceType.Asset;
                    default:
                        return ResourceType.Unknown;
                }
            }
        }

        public Task<IResource> GotoResource()
        {
            switch (link.RelationshipType)
            {
                case "self":
                    return Task<IResource>.FromResult((IResource)new AtomItemRoutable(item));
                case "enclosure":
                case "icon":
                default:
                    return WebRoute.Create(link.Uri).GotoResource();
            }
        }
    }
}